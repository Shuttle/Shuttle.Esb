using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;

namespace Shuttle.Esb
{
    public class AcknowledgeMessageObserver :
        IPipelineObserver<OnAcknowledgeMessage>
    {
        public void Execute(OnAcknowledgeMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            if (pipelineEvent.Pipeline.Exception != null && !state.GetTransactionComplete())
            {
                return;
            }

            state.GetWorkQueue().Acknowledge(state.GetReceivedMessage().AcknowledgementToken);
        }
    }
}