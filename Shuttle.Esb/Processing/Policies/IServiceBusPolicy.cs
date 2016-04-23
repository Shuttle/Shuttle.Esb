using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public interface IServiceBusPolicy
	{
		MessageFailureAction EvaluateMessageHandlingFailure(OnPipelineException pipelineEvent);
		MessageFailureAction EvaluateMessageDistributionFailure(OnPipelineException pipelineEvent);
		MessageFailureAction EvaluateOutboxFailure(OnPipelineException pipelineEvent);
	}
}