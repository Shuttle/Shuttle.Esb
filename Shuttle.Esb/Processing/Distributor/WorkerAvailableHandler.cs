using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class WorkerAvailableHandler : IMessageHandler<WorkerThreadAvailableCommand>
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IWorkerAvailabilityService _workerAvailabilityService;

        public WorkerAvailableHandler(IServiceBusConfiguration configuration, IWorkerAvailabilityService workerAvailabilityService)
        {
            Guard.AgainstNull(workerAvailabilityService, nameof(workerAvailabilityService));
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;
            _workerAvailabilityService = workerAvailabilityService;
        }

        public void ProcessMessage(IHandlerContext<WorkerThreadAvailableCommand> context)
        {
            var distributeSendCount = _configuration.Inbox.DistributeSendCount > 0
                ? _configuration.Inbox.DistributeSendCount
                : 5;

            _workerAvailabilityService.RemoveByThread(context.Message);

            for (var i = 0; i < distributeSendCount; i++)
            {
                _workerAvailabilityService.WorkerAvailable(context.Message);
            }
        }
    }
}