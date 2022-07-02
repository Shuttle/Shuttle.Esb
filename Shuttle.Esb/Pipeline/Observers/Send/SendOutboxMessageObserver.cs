using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb
{
    public interface ISendOutboxMessageObserver : IPipelineObserver<OnDispatchTransportMessage>
    {
    }

    public class SendOutboxMessageObserver : ISendOutboxMessageObserver
    {
        private readonly IBrokerEndpointService _brokerEndpointService;

        public SendOutboxMessageObserver(IBrokerEndpointService brokerEndpointService)
        {
            Guard.AgainstNull(brokerEndpointService, nameof(brokerEndpointService));

            _brokerEndpointService = brokerEndpointService;
        }

        public void Execute(OnDispatchTransportMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();
            var receivedMessage = state.GetReceivedMessage();

            Guard.AgainstNull(transportMessage, nameof(transportMessage));
            Guard.AgainstNull(receivedMessage, nameof(receivedMessage));
            Guard.AgainstNullOrEmptyString(transportMessage.RecipientUri, "uri");

            var queue = _brokerEndpointService.Get(transportMessage.RecipientUri);

            using (var stream = receivedMessage.Stream.Copy())
            {
                queue.Send(transportMessage, stream);
            }
        }
    }
}