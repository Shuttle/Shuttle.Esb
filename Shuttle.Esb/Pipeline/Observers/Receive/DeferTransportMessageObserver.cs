using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb;

public interface IDeferTransportMessageObserver : IPipelineObserver<OnAfterDeserializeTransportMessage>
{
    event EventHandler<TransportMessageDeferredEventArgs> TransportMessageDeferred;
}

public class DeferTransportMessageObserver : IDeferTransportMessageObserver
{
    private readonly IDeferredMessageProcessor _deferredMessageProcessor;

    public DeferTransportMessageObserver(IDeferredMessageProcessor deferredMessageProcessor)
    {
        _deferredMessageProcessor = Guard.AgainstNull(deferredMessageProcessor);
    }

    public event EventHandler<TransportMessageDeferredEventArgs>? TransportMessageDeferred;

    public async Task ExecuteAsync(IPipelineContext<OnAfterDeserializeTransportMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var transportMessage = Guard.AgainstNull(state.GetTransportMessage());
        var workQueue = Guard.AgainstNull(state.GetWorkQueue());

        if (!transportMessage.IsIgnoring() || workQueue.IsStream)
        {
            return;
        }

        var receivedMessage = Guard.AgainstNull(state.GetReceivedMessage());
        var deferredQueue = state.GetDeferredQueue();

        await using (var stream = await receivedMessage.Stream.CopyAsync().ConfigureAwait(false))
        {
            if (deferredQueue == null)
            {
                await workQueue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);
            }
            else
            {
                await deferredQueue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);

                await _deferredMessageProcessor.MessageDeferredAsync(transportMessage.IgnoreTillDate).ConfigureAwait(false);
            }
        }

        await workQueue.AcknowledgeAsync(receivedMessage.AcknowledgementToken).ConfigureAwait(false);

        TransportMessageDeferred?.Invoke(this, new(transportMessage));

        pipelineContext.Pipeline.Abort();
    }
}