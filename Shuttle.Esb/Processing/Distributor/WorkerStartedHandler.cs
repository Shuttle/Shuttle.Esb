using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class WorkerStartedHandler : IMessageHandler<WorkerStartedEvent>
    {
        private readonly IWorkerAvailabilityService _workerAvailabilityService;

        public WorkerStartedHandler(IWorkerAvailabilityService workerAvailabilityService)
        {
            Guard.AgainstNull(workerAvailabilityService, nameof(workerAvailabilityService));

            _workerAvailabilityService = workerAvailabilityService;
        }

        public void ProcessMessage(IHandlerContext<WorkerStartedEvent> context)
        {
            _workerAvailabilityService.WorkerStarted(context.Message);
        }
    }
}