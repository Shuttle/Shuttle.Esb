using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public interface IGetDeferredMessageObserver : IPipelineObserver<OnGetMessage>
{
}

public class GetDeferredMessageObserver : IGetDeferredMessageObserver
{
    public async Task ExecuteAsync(IPipelineContext<OnGetMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var queue = Guard.AgainstNull(state.GetDeferredQueue());

        var receivedMessage = await queue.GetMessageAsync().ConfigureAwait(false);

        // Abort the pipeline if there is no message on the queue
        if (receivedMessage == null)
        {
            pipelineContext.Pipeline.Abort();
        }
        else
        {
            state.SetWorking();
            state.SetReceivedMessage(receivedMessage);
        }
    }
}