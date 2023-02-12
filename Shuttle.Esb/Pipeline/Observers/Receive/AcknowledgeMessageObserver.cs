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
        public async Task Execute(OnAcknowledgeMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            if (pipelineEvent.Pipeline.Exception != null && !state.GetTransactionComplete())
            {
                return;
            }

            await state.GetWorkQueue().Acknowledge(state.GetReceivedMessage().AcknowledgementToken).ConfigureAwait(false);
        }
    }
}