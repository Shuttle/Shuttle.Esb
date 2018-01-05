using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class DispatchTransportMessagePipeline : Pipeline
    {
        public DispatchTransportMessagePipeline(IEnumerable<IPipelineObserver> observers)
        {
            Guard.AgainstNull(observers, nameof(observers));

            var list = observers.ToList();

            RegisterStage("Send")
                .WithEvent<OnFindRouteForMessage>()
                .WithEvent<OnAfterFindRouteForMessage>()
                .WithEvent<OnSerializeTransportMessage>()
                .WithEvent<OnAfterSerializeTransportMessage>()
                .WithEvent<OnDispatchTransportMessage>()
                .WithEvent<OnAfterDispatchTransportMessage>();

            RegisterObserver(list.Get<IFindMessageRouteObserver>());
            RegisterObserver(list.Get<ISerializeTransportMessageObserver>());
            RegisterObserver(list.Get<IDispatchTransportMessageObserver>());
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