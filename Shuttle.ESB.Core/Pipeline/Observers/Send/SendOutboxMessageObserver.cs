using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
	public class SendOutboxMessageObserver : IPipelineObserver<OnDispatchTransportMessage>
	{
		private readonly ILog _log;

		public SendOutboxMessageObserver()
		{
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

			var queue =
				state.GetServiceBus().Configuration.QueueManager.GetQueue(transportMessage.RecipientInboxWorkQueueUri);

			using (var stream = receivedMessage.Stream.Copy())
			{
				queue.Enqueue(transportMessage.MessageId, stream);
			}
		}
	}
}