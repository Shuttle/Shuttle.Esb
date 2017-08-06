using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DispatchTransportMessagePipeline : Pipeline
    {
        public DispatchTransportMessagePipeline(FindMessageRouteObserver findMessageRouteObserver,
            SerializeTransportMessageObserver serializeTransportMessageObserver,
            DispatchTransportMessageObserver dispatchTransportMessageObserver)
        {
            RegisterStage("Send")
                .WithEvent<OnFindRouteForMessage>()
                .WithEvent<OnAfterFindRouteForMessage>()
                .WithEvent<OnSerializeTransportMessage>()
                .WithEvent<OnAfterSerializeTransportMessage>()
                .WithEvent<OnDispatchTransportMessage>()
                .WithEvent<OnAfterDispatchTransportMessage>();

            RegisterObserver(findMessageRouteObserver);
            RegisterObserver(serializeTransportMessageObserver);
            RegisterObserver(dispatchTransportMessageObserver);
        }

        public bool Execute(TransportMessage transportMessage, TransportMessage transportMessageReceived)
        {
            Guard.AgainstNull(transportMessage, nameof(transportMessage));

            State.SetTransportMessage(transportMessage);
            State.SetTransportMessageReceived(transportMessageReceived);

            return base.Execute();
        }
    }
}