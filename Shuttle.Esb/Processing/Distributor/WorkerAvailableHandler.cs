using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class WorkerAvailableHandler : IMessageHandler<WorkerThreadAvailableCommand>
    {
        private readonly IWorkerAvailabilityService _workerAvailabilityService;
        private readonly ServiceBusOptions _options;

        public WorkerAvailableHandler(IOptions<ServiceBusOptions> options, IWorkerAvailabilityService workerAvailabilityService)
        {
            Guard.AgainstNull(options, nameof(options));
            Guard.AgainstNull(options.Value, nameof(options.Value));
            Guard.AgainstNull(workerAvailabilityService, nameof(workerAvailabilityService));

            _options = options.Value;
            _workerAvailabilityService = workerAvailabilityService;
        }

        public void ProcessMessage(IHandlerContext<WorkerThreadAvailableCommand> context)
        {
            var distributeSendCount = _options.Inbox.DistributeSendCount > 0
                ? _options.Inbox.DistributeSendCount
                : 5;

            _workerAvailabilityService.RemoveByThread(context.Message);

            for (var i = 0; i < distributeSendCount; i++)
            {
                _workerAvailabilityService.WorkerAvailable(context.Message);
            }
        }
    }
}