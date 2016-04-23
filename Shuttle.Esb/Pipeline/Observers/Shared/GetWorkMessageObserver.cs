using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class GetWorkMessageObserver : IPipelineObserver<OnGetMessage>
	{
		public void Execute(OnGetMessage pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var queue = state.GetWorkQueue();

			Guard.AgainstNull(queue, "workQueue");

			var receivedMessage = queue.GetMessage();

			// Abort the pipeline if there is no message on the queue
			if (receivedMessage == null)
			{
				state.GetServiceBus().Events.OnQueueEmpty(this, new QueueEmptyEventArgs(pipelineEvent, queue));
				pipelineEvent.Pipeline.Abort();
			}
			else
			{
				state.SetProcessingStatus(ProcessingStatus.Active);
				state.SetWorking();
				state.SetReceivedMessage(receivedMessage);
			}
		}
	}
}