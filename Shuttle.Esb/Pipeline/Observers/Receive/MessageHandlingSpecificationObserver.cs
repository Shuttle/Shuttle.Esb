using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public interface IMessageHandlingSpecificationObserver : IPipelineObserver<OnEvaluateMessageHandling>
{
}

public class MessageHandlingSpecificationObserver : IMessageHandlingSpecificationObserver
{
    private readonly IMessageHandlingSpecification _messageHandlingSpecification;

    public MessageHandlingSpecificationObserver(IMessageHandlingSpecification messageHandlingSpecification)
    {
        _messageHandlingSpecification = Guard.AgainstNull(messageHandlingSpecification);
    }

    public void Execute(OnEvaluateMessageHandling pipelineEvent)
    {
    }

    public async Task ExecuteAsync(IPipelineContext<OnEvaluateMessageHandling> pipelineContext)
    {
        Guard.AgainstNull(pipelineContext);

        var state = pipelineContext.Pipeline.State;

        if (_messageHandlingSpecification.IsSatisfiedBy(pipelineContext))
        {
            return;
        }

        state.SetProcessingStatus(ProcessingStatus.Ignore);

        await Task.CompletedTask.ConfigureAwait(false);
    }
}