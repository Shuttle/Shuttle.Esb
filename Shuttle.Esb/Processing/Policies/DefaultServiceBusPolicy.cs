using System;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public class DefaultServiceBusPolicy : IServiceBusPolicy
{
    public MessageFailureAction EvaluateMessageHandlingFailure(IPipelineContext<OnPipelineException> pipelineContext)
    {
        return DefaultEvaluation(pipelineContext);
    }

    public MessageFailureAction EvaluateOutboxFailure(IPipelineContext<OnPipelineException> pipelineContext)
    {
        return DefaultEvaluation(pipelineContext);
    }

    private MessageFailureAction DefaultEvaluation(IPipelineContext<OnPipelineException> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var transportMessage = Guard.AgainstNull(state.GetTransportMessage());
        var durationToIgnoreOnFailure = Guard.AgainstNull(state.GetDurationToIgnoreOnFailure()).ToArray();

        TimeSpan timeSpanToIgnoreRetriedMessage;

        var failureIndex = transportMessage.FailureMessages.Count + 1;
        var retry = failureIndex < state.GetMaximumFailureCount();

        if (!retry || durationToIgnoreOnFailure.Length == 0)
        {
            timeSpanToIgnoreRetriedMessage = TimeSpan.Zero;
        }
        else
        {
            timeSpanToIgnoreRetriedMessage = durationToIgnoreOnFailure.Length < failureIndex
                ? durationToIgnoreOnFailure[^1]
                : durationToIgnoreOnFailure[failureIndex - 1];
        }

        return new(retry, timeSpanToIgnoreRetriedMessage);
    }
}