﻿using System;
using System.Linq;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class DefaultServiceBusPolicy : IServiceBusPolicy
    {
        public MessageFailureAction EvaluateMessageHandlingFailure(OnPipelineException pipelineEvent)
        {
            return DefaultEvaluation(pipelineEvent);
        }

        public MessageFailureAction EvaluateMessageDistributionFailure(OnPipelineException pipelineEvent)
        {
            return DefaultEvaluation(pipelineEvent);
        }

        public MessageFailureAction EvaluateOutboxFailure(OnPipelineException pipelineEvent)
        {
            return DefaultEvaluation(pipelineEvent);
        }

        private MessageFailureAction DefaultEvaluation(OnPipelineException pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();
            var durationToIgnoreOnFailure = state.GetDurationToIgnoreOnFailure().ToArray();

            TimeSpan timeSpanToIgnoreRetriedMessage;

            var failureIndex = transportMessage.FailureMessages.Count + 1;
            var retry = failureIndex < state.GetMaximumFailureCount();

            if (!retry || durationToIgnoreOnFailure == null || durationToIgnoreOnFailure.Length == 0)
            {
                timeSpanToIgnoreRetriedMessage = TimeSpan.Zero;
            }
            else
            {
                timeSpanToIgnoreRetriedMessage = durationToIgnoreOnFailure.Length <
                                                 failureIndex
                    ? durationToIgnoreOnFailure[durationToIgnoreOnFailure.Length - 1]
                    : durationToIgnoreOnFailure[failureIndex - 1];
            }

            return new MessageFailureAction(
                retry,
                timeSpanToIgnoreRetriedMessage);
        }
    }
}