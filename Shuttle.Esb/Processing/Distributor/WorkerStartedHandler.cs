namespace Shuttle.Esb
{
	public class WorkerStartedHandler : IMessageHandler<WorkerStartedEvent>
	{
		public void ProcessMessage(IHandlerContext<WorkerStartedEvent> context)
		{
			context.Configuration.WorkerAvailabilityManager.WorkerStarted(context.Message);
		}

		public bool IsReusable
		{
			get { return true; }
		}
	}
}