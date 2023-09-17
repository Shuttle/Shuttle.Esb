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
        private async Task ExecuteAsync(OnProcessDeferredMessage pipelineEvent, bool sync)
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
                if (sync)
                {
                    deferredQueue.Release(receivedMessage.AcknowledgementToken);
                }
                else
                {
                    await deferredQueue.ReleaseAsync(receivedMessage.AcknowledgementToken).ConfigureAwait(false);
                }

                state.SetDeferredMessageReturned(false);

                return;
            }

            if (sync)
            {
                workQueue.Enqueue(transportMessage, receivedMessage.Stream);
                deferredQueue.Acknowledge(receivedMessage.AcknowledgementToken);
            }
            else
            {
                await workQueue.EnqueueAsync(transportMessage, receivedMessage.Stream).ConfigureAwait(false);
                await deferredQueue.AcknowledgeAsync(receivedMessage.AcknowledgementToken).ConfigureAwait(false);
            }

            state.SetDeferredMessageReturned(true);
        }

        public void Execute(OnProcessDeferredMessage pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnProcessDeferredMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }
    }
}