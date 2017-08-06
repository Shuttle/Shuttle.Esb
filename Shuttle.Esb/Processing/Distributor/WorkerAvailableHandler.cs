using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class WorkerAvailableHandler : IMessageHandler<WorkerThreadAvailableCommand>
    {
        private readonly IWorkerAvailabilityManager _workerAvailabilityManager;

        public WorkerAvailableHandler(IWorkerAvailabilityManager workerAvailabilityManager)
        {
            Guard.AgainstNull(workerAvailabilityManager, nameof(workerAvailabilityManager));

            _workerAvailabilityManager = workerAvailabilityManager;
        }

        public void ProcessMessage(IHandlerContext<WorkerThreadAvailableCommand> context)
        {
            var distributeSendCount = context.Configuration.Inbox.DistributeSendCount > 0
                ? context.Configuration.Inbox.DistributeSendCount
                : 5;

            _workerAvailabilityManager.RemoveByThread(context.Message);

            for (var i = 0; i < distributeSendCount; i++)
            {
                _workerAvailabilityManager.WorkerAvailable(context.Message);
            }
        }
    }
}