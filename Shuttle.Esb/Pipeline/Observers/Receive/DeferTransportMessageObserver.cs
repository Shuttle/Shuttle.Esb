using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DeferTransportMessageObserver : IPipelineObserver<OnAfterDeserializeTransportMessage>
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly ILog _log;

        public DeferTransportMessageObserver(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;
            _log = Log.For(this);
        }

        public void Execute(OnAfterDeserializeTransportMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var receivedMessage = state.GetReceivedMessage();
            var transportMessage = state.GetTransportMessage();
            var workQueue = state.GetWorkQueue();

            Guard.AgainstNull(receivedMessage, nameof(receivedMessage));
            Guard.AgainstNull(transportMessage, nameof(transportMessage));
            Guard.AgainstNull(workQueue, nameof(workQueue));

            if (!transportMessage.IsIgnoring())
            {
                return;
            }

            using (var stream = receivedMessage.Stream.Copy())
            {
                if (state.GetDeferredQueue() == null)
                {
                    workQueue.Enqueue(transportMessage, stream);
                }
                else
                {
                    state.GetDeferredQueue().Enqueue(transportMessage, stream);

                    _configuration.Inbox.DeferredMessageProcessor.MessageDeferred(transportMessage.IgnoreTillDate);
                }
            }

            workQueue.Acknowledge(receivedMessage.AcknowledgementToken);

            if (_log.IsTraceEnabled)
            {
                _log.Trace(string.Format(EsbResources.TraceTransportMessageDeferred, transportMessage.MessageId,
                    transportMessage.IgnoreTillDate.ToString("O")));
            }

            pipelineEvent.Pipeline.Abort();
        }
    }
}