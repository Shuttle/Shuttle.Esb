using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class ProcessDeferredMessageObserver : IPipelineObserver<OnProcessDeferredMessage>
    {
        public void Execute(OnProcessDeferredMessage pipelineEvent)
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
                deferredQueue.Release(receivedMessage.AcknowledgementToken);

                state.SetDeferredMessageReturned(false);

                return;
            }

            workQueue.Enqueue(transportMessage, receivedMessage.Stream);
            deferredQueue.Acknowledge(receivedMessage.AcknowledgementToken);

            state.SetDeferredMessageReturned(true);
        }
    }
}