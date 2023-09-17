using System.Threading.Tasks;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;

namespace Shuttle.Esb
{
    public interface IAcknowledgeMessageObserver : IPipelineObserver<OnAcknowledgeMessage>
    {
    }

    public class AcknowledgeMessageObserver : IAcknowledgeMessageObserver
    {
        public void Execute(OnAcknowledgeMessage pipelineEvent)
        {
            throw new System.NotImplementedException();
        }

        public async Task ExecuteAsync(OnAcknowledgeMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            if (pipelineEvent.Pipeline.Exception != null && !state.GetTransactionComplete())
            {
                return;
            }

            await state.GetWorkQueue().AcknowledgeAsync(state.GetReceivedMessage().AcknowledgementToken).ConfigureAwait(false);
        }
    }
}