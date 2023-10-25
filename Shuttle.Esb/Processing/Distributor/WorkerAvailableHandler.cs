using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class WorkerAvailableHandler :
        IMessageHandler<WorkerThreadAvailableCommand>,
        IAsyncMessageHandler<WorkerThreadAvailableCommand>
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

        public void ProcessMessage(IHandlerContext<WorkerThreadAvailableCommand> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var distributeSendCount = _serviceBusOptions.Inbox.DistributeSendCount > 0
                ? _serviceBusOptions.Inbox.DistributeSendCount
                : 5;

            _workerAvailabilityService.RemoveByThread(context.Message);

            for (var i = 0; i < distributeSendCount; i++)
            {
                _workerAvailabilityService.WorkerAvailable(context.Message);
            }
        }

        public async Task ProcessMessageAsync(IHandlerContext<WorkerThreadAvailableCommand> context)
        {
            ProcessMessage(context);

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}