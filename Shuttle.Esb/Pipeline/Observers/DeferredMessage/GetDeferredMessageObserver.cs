using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IGetDeferredMessageObserver : IPipelineObserver<OnGetMessage>
    {
    }

    public class GetDeferredMessageObserver : IGetDeferredMessageObserver
    {
        private async Task ExecuteAsync(OnGetMessage pipelineEvent, bool sync)
        {
            var state = pipelineEvent.Pipeline.State;
            var queue = state.GetDeferredQueue();

            Guard.AgainstNull(queue, nameof(queue));

            var receivedMessage = sync
                ? queue.GetMessage()
                : await queue.GetMessageAsync().ConfigureAwait(false);

            // Abort the pipeline if there is no message on the queue
            if (receivedMessage == null)
            {
                pipelineEvent.Pipeline.Abort();
            }
            else
            {
                state.SetWorking();
                state.SetReceivedMessage(receivedMessage);
            }
        }

        public void Execute(OnGetMessage pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnGetMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }
    }
}