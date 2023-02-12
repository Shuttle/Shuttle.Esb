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
        public async Task Execute(OnGetMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var queue = state.GetDeferredQueue();

            Guard.AgainstNull(queue, nameof(queue));

            var receivedMessage = await queue.GetMessage().ConfigureAwait(false);

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
    }
}