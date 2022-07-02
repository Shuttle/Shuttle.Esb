using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb
{
    public interface IDeferTransportMessageObserver : IPipelineObserver<OnAfterDeserializeTransportMessage>
    {
    }

    public class DeferTransportMessageObserver : IDeferTransportMessageObserver
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
            var brokerEndpoint = state.GetBrokerEndpoint();

            Guard.AgainstNull(receivedMessage, nameof(receivedMessage));
            Guard.AgainstNull(transportMessage, nameof(transportMessage));
            Guard.AgainstNull(brokerEndpoint, nameof(brokerEndpoint));

            if (!transportMessage.IsIgnoring())
            {
                return;
            }

            using (var stream = receivedMessage.Stream.Copy())
            {
                if (state.GetDeferredBrokerEndpoint() == null)
                {
                    brokerEndpoint.Enqueue(transportMessage, stream);
                }
                else
                {
                    state.GetDeferredBrokerEndpoint().Enqueue(transportMessage, stream);

                    _configuration.Inbox.DeferredMessageProcessor.MessageDeferred(transportMessage.IgnoreTillDate);
                }
            }

            brokerEndpoint.Acknowledge(receivedMessage.AcknowledgementToken);

            if (_log.IsTraceEnabled)
            {
                _log.Trace(string.Format(Resources.TraceTransportMessageDeferred, transportMessage.MessageId,
                    transportMessage.IgnoreTillDate.ToString("O")));
            }

            pipelineEvent.Pipeline.Abort();
        }
    }
}