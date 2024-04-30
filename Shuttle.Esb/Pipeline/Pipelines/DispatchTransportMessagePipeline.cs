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

        public bool Execute(TransportMessage transportMessage, TransportMessage transportMessageReceived, CancellationToken cancellationToken = default)
        {
            return ExecuteAsync(transportMessage, transportMessageReceived, cancellationToken, true).GetAwaiter().GetResult();
        }

        public async Task<bool> ExecuteAsync(TransportMessage transportMessage, TransportMessage transportMessageReceived, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(transportMessage, transportMessageReceived, cancellationToken, false).ConfigureAwait(false);
        }

        private async Task<bool> ExecuteAsync(TransportMessage transportMessage, TransportMessage transportMessageReceived, CancellationToken cancellationToken, bool sync)
        {
            Guard.AgainstNull(transportMessage, nameof(transportMessage));

            State.SetTransportMessage(transportMessage);
            State.SetTransportMessageReceived(transportMessageReceived);

            return sync 
            ? base.Execute(cancellationToken)
            : await base.ExecuteAsync(cancellationToken);
        }
    }
}