using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IProcessDeferredMessageObserver : IPipelineObserver<OnProcessDeferredMessage>
    {
    }

    public class ProcessDeferredMessageObserver : IProcessDeferredMessageObserver
    {
        public void Execute(OnProcessDeferredMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();
            var receivedMessage = state.GetReceivedMessage();
            var brokerEndpoint = state.GetBrokerEndpoint();
            var deferredBrokerEndpoint = state.GetDeferredBrokerEndpoint();

            Guard.AgainstNull(transportMessage, nameof(transportMessage));
            Guard.AgainstNull(receivedMessage, nameof(receivedMessage));
            Guard.AgainstNull(brokerEndpoint, nameof(brokerEndpoint));
            Guard.AgainstNull(deferredBrokerEndpoint, nameof(deferredBrokerEndpoint));

            if (transportMessage.IsIgnoring())
            {
                deferredBrokerEndpoint.Release(receivedMessage.AcknowledgementToken);

                state.SetDeferredMessageReturned(false);

                return;
            }

            brokerEndpoint.Send(transportMessage, receivedMessage.Stream);
            deferredBrokerEndpoint.Acknowledge(receivedMessage.AcknowledgementToken);

            state.SetDeferredMessageReturned(true);
        }
    }
}