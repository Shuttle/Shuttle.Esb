using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransactionScope;

namespace Shuttle.Esb;

public interface IAcknowledgeMessageObserver : IPipelineObserver<OnAcknowledgeMessage>
{
}

public class AcknowledgeMessageObserver : IAcknowledgeMessageObserver
{
    public async Task ExecuteAsync(IPipelineContext<OnAcknowledgeMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;

        if (pipelineContext.Pipeline.Exception != null && !state.GetTransactionScopeCompleted())
        {
            return;
        }

        var acknowledgementToken = Guard.AgainstNull(state.GetReceivedMessage()).AcknowledgementToken;

        await Guard.AgainstNull(state.GetWorkQueue()).AcknowledgeAsync(acknowledgementToken).ConfigureAwait(false);
    }
}