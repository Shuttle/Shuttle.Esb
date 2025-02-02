using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public interface IProcessDeferredMessageObserver : IPipelineObserver<OnProcessDeferredMessage>
{
    event EventHandler<MessageReturnedEventArgs>? MessageReturned;
}

public class ProcessDeferredMessageObserver : IProcessDeferredMessageObserver
{
    public event EventHandler<MessageReturnedEventArgs>? MessageReturned;

    public async Task ExecuteAsync(IPipelineContext<OnProcessDeferredMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var transportMessage = Guard.AgainstNull(state.GetTransportMessage());
        var receivedMessage = Guard.AgainstNull(state.GetReceivedMessage());
        var workQueue = Guard.AgainstNull(state.GetWorkQueue());
        var deferredQueue = Guard.AgainstNull(state.GetDeferredQueue());

        if (transportMessage.IsIgnoring())
        {
            await deferredQueue.ReleaseAsync(receivedMessage.AcknowledgementToken).ConfigureAwait(false);

            state.SetDeferredMessageReturned(false);

            return;
        }

        await workQueue.EnqueueAsync(transportMessage, receivedMessage.Stream).ConfigureAwait(false);
        await deferredQueue.AcknowledgeAsync(receivedMessage.AcknowledgementToken).ConfigureAwait(false);

        state.SetDeferredMessageReturned(true);

        MessageReturned?.Invoke(this, new(transportMessage, receivedMessage));
    }
}