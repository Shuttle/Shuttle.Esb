using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IServiceBusPolicy
    {
        MessageFailureAction EvaluateMessageHandlingFailure(OnPipelineException pipelineEvent);
        MessageFailureAction EvaluateOutboxFailure(OnPipelineException pipelineEvent);
    }
}