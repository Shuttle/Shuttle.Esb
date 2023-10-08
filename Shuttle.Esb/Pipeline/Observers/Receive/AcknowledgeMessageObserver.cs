using System.Threading.Tasks;
using Shuttle.Core.Contract;
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
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnAcknowledgeMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnAcknowledgeMessage pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;

            if (pipelineEvent.Pipeline.Exception != null && !state.GetTransactionComplete())
            {
                return;
            }

            var acknowledgementToken = Guard.AgainstNull(state.GetReceivedMessage(), StateKeys.ReceivedMessage).AcknowledgementToken;

            if (sync)
            {
                state.GetWorkQueue().Acknowledge(acknowledgementToken);
            }
            else
            {
                await state.GetWorkQueue().AcknowledgeAsync(acknowledgementToken).ConfigureAwait(false);
            }
        }
    }
}