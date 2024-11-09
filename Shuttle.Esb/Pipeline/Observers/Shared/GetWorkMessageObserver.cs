using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public interface IGetWorkMessageObserver : IPipelineObserver<OnGetMessage>
{
}

public class GetWorkMessageObserver : IGetWorkMessageObserver
{
    public async Task ExecuteAsync(IPipelineContext<OnGetMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var queue = Guard.AgainstNull(state.GetWorkQueue());

        var receivedMessage = await queue.GetMessageAsync().ConfigureAwait(false);

        if (receivedMessage == null)
        {
            pipelineContext.Pipeline.Abort();

            return;
        }
        
        state.SetProcessingStatus(ProcessingStatus.Active);
        state.SetWorking();
        state.SetReceivedMessage(receivedMessage);
    }
}