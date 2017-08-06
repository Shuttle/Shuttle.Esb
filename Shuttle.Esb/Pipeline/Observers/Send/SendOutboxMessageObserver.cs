using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class SendOutboxMessageObserver : IPipelineObserver<OnDispatchTransportMessage>
    {
        private readonly ILog _log;
        private readonly IQueueManager _queueManager;

        public SendOutboxMessageObserver(IQueueManager queueManager)
        {
            Guard.AgainstNull(queueManager, nameof(queueManager));

            _queueManager = queueManager;
            _log = Log.For(this);
        }

        public void Execute(OnDispatchTransportMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();
            var receivedMessage = state.GetReceivedMessage();

            Guard.AgainstNull(transportMessage, nameof(transportMessage));
            Guard.AgainstNull(receivedMessage, nameof(receivedMessage));
            Guard.AgainstNullOrEmptyString(transportMessage.RecipientInboxWorkQueueUri, "uri");

            var queue = _queueManager.GetQueue(transportMessage.RecipientInboxWorkQueueUri);

            using (var stream = receivedMessage.Stream.Copy())
            {
                queue.Enqueue(transportMessage, stream);
            }
        }
    }
}