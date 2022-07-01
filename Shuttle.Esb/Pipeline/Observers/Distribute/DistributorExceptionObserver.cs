using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb
{
    public interface IDistributorExceptionObserver : IPipelineObserver<OnPipelineException>
    {
    }

    public class DistributorExceptionObserver : IDistributorExceptionObserver
    {
        private readonly IServiceBusEvents _events;
        private readonly IServiceBusPolicy _policy;
        private readonly ISerializer _serializer;

        public DistributorExceptionObserver(IServiceBusEvents events, IServiceBusPolicy policy, ISerializer serializer)
        {
            Guard.AgainstNull(events, nameof(events));
            Guard.AgainstNull(policy, nameof(policy));
            Guard.AgainstNull(serializer, nameof(serializer));

            _events = events;
            _policy = policy;
            _serializer = serializer;
        }

        public void Execute(OnPipelineException pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            _events.OnBeforePipelineExceptionHandled(this, new PipelineExceptionEventArgs(pipelineEvent.Pipeline));

            try
            {
                if (pipelineEvent.Pipeline.ExceptionHandled)
                {
                    return;
                }

                try
                {
                    var transportMessage = state.GetTransportMessage();

                    if (transportMessage == null)
                    {
                        return;
                    }

                    var action = _policy.EvaluateMessageDistributionFailure(pipelineEvent);

                    transportMessage.RegisterFailure(pipelineEvent.Pipeline.Exception.AllMessages(),
                        action.TimeSpanToIgnoreRetriedMessage);

                    if (action.Retry)
                    {
                        state.GetBrokerEndpoint().Enqueue(transportMessage, _serializer.Serialize(transportMessage));
                    }
                    else
                    {
                        state.GetErrorBrokerEndpoint().Enqueue(transportMessage, _serializer.Serialize(transportMessage));
                    }

                    state.GetBrokerEndpoint().Acknowledge(state.GetReceivedMessage().AcknowledgementToken);
                }
                finally
                {
                    pipelineEvent.Pipeline.MarkExceptionHandled();
                    _events.OnAfterPipelineExceptionHandled(this,
                        new PipelineExceptionEventArgs(pipelineEvent.Pipeline));
                }
            }
            finally
            {
                pipelineEvent.Pipeline.Abort();
            }
        }
    }
}