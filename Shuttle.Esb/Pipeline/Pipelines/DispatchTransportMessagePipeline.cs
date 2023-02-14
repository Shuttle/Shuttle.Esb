using System.Threading;
using System.Threading.Tasks;
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
            RegisterStage("Send")
                .WithEvent<OnFindRouteForMessage>()
                .WithEvent<OnAfterFindRouteForMessage>()
                .WithEvent<OnSerializeTransportMessage>()
                .WithEvent<OnAfterSerializeTransportMessage>()
                .WithEvent<OnDispatchTransportMessage>()
                .WithEvent<OnAfterDispatchTransportMessage>();

            RegisterObserver(Guard.AgainstNull(findMessageRouteObserver, nameof(findMessageRouteObserver)));
            RegisterObserver(Guard.AgainstNull(serializeTransportMessageObserver, nameof(serializeTransportMessageObserver)));
            RegisterObserver(Guard.AgainstNull(dispatchTransportMessageObserver, nameof(dispatchTransportMessageObserver)));
        }

        public Task<bool> Execute(TransportMessage transportMessage, TransportMessage transportMessageReceived, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(transportMessage, nameof(transportMessage));

            State.SetTransportMessage(transportMessage);
            State.SetTransportMessageReceived(transportMessageReceived);

            return base.Execute(cancellationToken);
        }
    }
}