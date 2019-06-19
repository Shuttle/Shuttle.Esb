using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class WorkerAvailableHandler : IMessageHandler<WorkerThreadAvailableCommand>
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IWorkerAvailabilityManager _workerAvailabilityManager;

        public WorkerAvailableHandler(IServiceBusConfiguration configuration, IWorkerAvailabilityManager workerAvailabilityManager)
        {
            Guard.AgainstNull(workerAvailabilityManager, nameof(workerAvailabilityManager));
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;
            _workerAvailabilityManager = workerAvailabilityManager;
        }

        public void ProcessMessage(IHandlerContext<WorkerThreadAvailableCommand> context)
        {
            var distributeSendCount = _configuration.Inbox.DistributeSendCount > 0
                ? _configuration.Inbox.DistributeSendCount
                : 5;

            _workerAvailabilityManager.RemoveByThread(context.Message);

            for (var i = 0; i < distributeSendCount; i++)
            {
                _workerAvailabilityManager.WorkerAvailable(context.Message);
            }
        }
    }
}