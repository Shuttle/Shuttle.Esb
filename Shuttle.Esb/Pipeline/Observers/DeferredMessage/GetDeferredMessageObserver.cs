using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class GetDeferredMessageObserver : IPipelineObserver<OnGetMessage>
	{
	    private readonly IServiceBusEvents _events;

	    public GetDeferredMessageObserver(IServiceBusEvents events)
	    {
            Guard.AgainstNull(events, "events");

	        _events = events;
	    }

	    public void Execute(OnGetMessage pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var queue = state.GetDeferredQueue();

			Guard.AgainstNull(queue, "deferredQueue");

			var receivedMessage = queue.GetMessage();

			// Abort the pipeline if there is no message on the queue
			if (receivedMessage == null)
			{
				_events.OnQueueEmpty(this, new QueueEmptyEventArgs(pipelineEvent, queue));

				pipelineEvent.Pipeline.Abort();
			}
			else
			{
				state.SetWorking();
				state.SetReceivedMessage(receivedMessage);
			}
		}
	}
}