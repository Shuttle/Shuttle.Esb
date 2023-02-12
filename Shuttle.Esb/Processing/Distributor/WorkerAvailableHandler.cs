using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class WorkerAvailableHandler : IMessageHandler<WorkerThreadAvailableCommand>
    {
        private readonly IWorkerAvailabilityService _workerAvailabilityService;
        private readonly ServiceBusOptions _serviceBusOptions;

        public WorkerAvailableHandler(IOptions<ServiceBusOptions> serviceBusOptions, IWorkerAvailabilityService workerAvailabilityService)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));
            Guard.AgainstNull(workerAvailabilityService, nameof(workerAvailabilityService));

            _serviceBusOptions = serviceBusOptions.Value;
            _workerAvailabilityService = workerAvailabilityService;
        }

        public async Task ProcessMessage(IHandlerContext<WorkerThreadAvailableCommand> context)
        {
            var distributeSendCount = _serviceBusOptions.Inbox.DistributeSendCount > 0
                ? _serviceBusOptions.Inbox.DistributeSendCount
                : 5;

            _workerAvailabilityService.RemoveByThread(context.Message);

            for (var i = 0; i < distributeSendCount; i++)
            {
                _workerAvailabilityService.WorkerAvailable(context.Message);
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}