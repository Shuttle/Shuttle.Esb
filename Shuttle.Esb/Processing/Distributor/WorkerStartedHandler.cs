using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class WorkerStartedHandler : IMessageHandler<WorkerStartedEvent>
	{
	    private readonly IWorkerAvailabilityManager _workerAvailabilityManager;

	    public WorkerStartedHandler(IWorkerAvailabilityManager workerAvailabilityManager)
	    {
            Guard.AgainstNull(workerAvailabilityManager, "workerAvailabilityManager");

	        _workerAvailabilityManager = workerAvailabilityManager;
	    }

	    public void ProcessMessage(IHandlerContext<WorkerStartedEvent> context)
		{
			_workerAvailabilityManager.WorkerStarted(context.Message);
		}
	}
}