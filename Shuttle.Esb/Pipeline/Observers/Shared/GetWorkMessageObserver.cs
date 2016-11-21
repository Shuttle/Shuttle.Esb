using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class GetWorkMessageObserver : IPipelineObserver<OnGetMessage>
	{
        private readonly IServiceBusEvents _events;

	    public GetWorkMessageObserver(IServiceBusEvents events)
	    {
            Guard.AgainstNull(events,"events");

	        _events = events;
	    }

	    public void Execute(OnGetMessage pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var queue = state.GetWorkQueue();

			Guard.AgainstNull(queue, "workQueue");

			var receivedMessage = queue.GetMessage();

			// Abort the pipeline if there is no message on the queue
			if (receivedMessage == null)
			{
				_events.OnQueueEmpty(this, new QueueEmptyEventArgs(pipelineEvent, queue));
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