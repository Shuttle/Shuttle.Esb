using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class DispatchTransportMessagePipeline : Pipeline
    {
        public DispatchTransportMessagePipeline(IFindMessageRouteObserver findMessageRouteObserver,
            ISerializeTransportMessageObserver serializeTransportMessageObserver,
            IDispatchTransportMessageObserver dispatchTransportMessageObserver)
        {
            Guard.AgainstNull(findMessageRouteObserver, nameof(findMessageRouteObserver));
            Guard.AgainstNull(serializeTransportMessageObserver, nameof(serializeTransportMessageObserver));
            Guard.AgainstNull(dispatchTransportMessageObserver, nameof(dispatchTransportMessageObserver));

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