using System;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public class DispatchTransportMessagePipeline : Pipeline
{
    public DispatchTransportMessagePipeline(IServiceProvider serviceProvider, IFindMessageRouteObserver findMessageRouteObserver, ISerializeTransportMessageObserver serializeTransportMessageObserver, IDispatchTransportMessageObserver dispatchTransportMessageObserver) 
        : base(serviceProvider)
    {
        RegisterStage("Send")
            .WithEvent<OnFindRouteForMessage>()
            .WithEvent<OnAfterFindRouteForMessage>()
            .WithEvent<OnSerializeTransportMessage>()
            .WithEvent<OnAfterSerializeTransportMessage>()
            .WithEvent<OnDispatchTransportMessage>()
            .WithEvent<OnAfterDispatchTransportMessage>();

        RegisterObserver(Guard.AgainstNull(findMessageRouteObserver));
        RegisterObserver(Guard.AgainstNull(serializeTransportMessageObserver));
        RegisterObserver(Guard.AgainstNull(dispatchTransportMessageObserver));
    }

    public async Task<bool> ExecuteAsync(TransportMessage transportMessage, TransportMessage? transportMessageReceived, CancellationToken cancellationToken = default)
    {
        State.SetTransportMessage(Guard.AgainstNull(transportMessage));
        State.SetTransportMessageReceived(transportMessageReceived);

        return await base.ExecuteAsync(cancellationToken);
    }
}