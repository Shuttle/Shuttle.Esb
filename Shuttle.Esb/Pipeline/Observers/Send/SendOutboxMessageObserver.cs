using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class SendOutboxMessageObserver : IPipelineObserver<OnDispatchTransportMessage>
	{
	    private readonly IQueueManager _queueManager;
	    private readonly ILog _log;

		public SendOutboxMessageObserver(IQueueManager queueManager)
		{
            Guard.AgainstNull(queueManager, "queueManager");

		    _queueManager = queueManager;
		    _log = Log.For(this);
		}

	    public void Execute(OnDispatchTransportMessage pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var transportMessage = state.GetTransportMessage();
			var receivedMessage = state.GetReceivedMessage();

			Guard.AgainstNull(transportMessage, "transportMessage");
			Guard.AgainstNull(receivedMessage, "receivedMessage");
			Guard.AgainstNullOrEmptyString(transportMessage.RecipientInboxWorkQueueUri, "uri");

			var queue = _queueManager.GetQueue(transportMessage.RecipientInboxWorkQueueUri);

			using (var stream = receivedMessage.Stream.Copy())
			{
				queue.Enqueue(transportMessage, stream);
			}
		}
	}
}