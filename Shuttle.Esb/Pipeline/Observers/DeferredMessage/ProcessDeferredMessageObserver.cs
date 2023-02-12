using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IProcessDeferredMessageObserver : IPipelineObserver<OnProcessDeferredMessage>
    {
    }

    public class ProcessDeferredMessageObserver : IProcessDeferredMessageObserver
    {
        public async Task Execute(OnProcessDeferredMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();
            var receivedMessage = state.GetReceivedMessage();
            var workQueue = state.GetWorkQueue();
            var deferredQueue = state.GetDeferredQueue();

            Guard.AgainstNull(transportMessage, nameof(transportMessage));
            Guard.AgainstNull(receivedMessage, nameof(receivedMessage));
            Guard.AgainstNull(workQueue, nameof(workQueue));
            Guard.AgainstNull(deferredQueue, nameof(deferredQueue));

            if (transportMessage.IsIgnoring())
            {
                await deferredQueue.Release(receivedMessage.AcknowledgementToken).ConfigureAwait(false);

                state.SetDeferredMessageReturned(false);

                return;
            }

            await workQueue.Enqueue(transportMessage, receivedMessage.Stream).ConfigureAwait(false);
            await deferredQueue.Acknowledge(receivedMessage.AcknowledgementToken).ConfigureAwait(false);

            state.SetDeferredMessageReturned(true);
        }
    }
}