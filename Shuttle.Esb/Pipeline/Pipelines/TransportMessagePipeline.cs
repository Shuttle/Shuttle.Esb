using System;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public class TransportMessagePipeline : Pipeline
{
    public TransportMessagePipeline(IServiceProvider serviceProvider, IAssembleMessageObserver assembleMessageObserver, ISerializeMessageObserver serializeMessageObserver, ICompressMessageObserver compressMessageObserver, IEncryptMessageObserver encryptMessageObserver)
        : base(serviceProvider)
    {
        AddStage("Create")
            .WithEvent<OnAssembleMessage>()
            .WithEvent<OnAfterAssembleMessage>()
            .WithEvent<OnSerializeMessage>()
            .WithEvent<OnAfterSerializeMessage>()
            .WithEvent<OnEncryptMessage>()
            .WithEvent<OnAfterEncryptMessage>()
            .WithEvent<OnCompressMessage>()
            .WithEvent<OnAfterCompressMessage>();

        AddObserver(Guard.AgainstNull(assembleMessageObserver));
        AddObserver(Guard.AgainstNull(serializeMessageObserver));
        AddObserver(Guard.AgainstNull(compressMessageObserver));
        AddObserver(Guard.AgainstNull(encryptMessageObserver));
    }

    public async Task<bool> ExecuteAsync(object message, TransportMessage? transportMessageReceived, Action<TransportMessageBuilder>? builder, CancellationToken cancellationToken = default)
    {
        State.SetMessage(Guard.AgainstNull(message));
        State.SetTransportMessageReceived(transportMessageReceived);
        State.SetTransportMessageBuilder(builder);

        return await base.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }
}