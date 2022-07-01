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
            var state = pipelineEvent.Pipeline.State;

            if (pipelineEvent.Pipeline.Exception != null && !state.GetTransactionComplete())
            {
                return;
            }

            state.GetBrokerEndpoint().Acknowledge(state.GetReceivedMessage().AcknowledgementToken);
        }
    }
}