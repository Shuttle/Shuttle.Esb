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
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var transportMessage = Guard.AgainstNull(state.GetTransportMessage(), StateKeys.TransportMessage);
            var receivedMessage = Guard.AgainstNull(state.GetReceivedMessage(), StateKeys.ReceivedMessage);
            var workQueue = Guard.AgainstNull(state.GetWorkQueue(), StateKeys.WorkQueue);
            var deferredQueue = Guard.AgainstNull(state.GetDeferredQueue(), StateKeys.DeferredQueue);

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