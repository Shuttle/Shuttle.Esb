using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DistributorExceptionObserver :
        IPipelineObserver<OnPipelineException>
    {
        private readonly IServiceBusPolicy _policy;
        private readonly ISerializer _serializer;

        public DistributorExceptionObserver(IServiceBusPolicy policy, ISerializer serializer)
        {
            Guard.AgainstNull(policy, "policy");
            Guard.AgainstNull(serializer, "serializer");

            _policy = policy;
            _serializer = serializer;
        }

        public void Execute(OnPipelineException pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var bus = state.GetServiceBus();

            bus.Events.OnBeforePipelineExceptionHandled(this, new PipelineExceptionEventArgs(pipelineEvent.Pipeline));

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
                        state.GetWorkQueue().Enqueue(transportMessage, _serializer.Serialize(transportMessage));
                    }
                    else
                    {
                        state.GetErrorQueue().Enqueue(transportMessage, _serializer.Serialize(transportMessage));
                    }

                    state.GetWorkQueue().Acknowledge(state.GetReceivedMessage().AcknowledgementToken);
                }
                finally
                {
                    pipelineEvent.Pipeline.MarkExceptionHandled();
                    bus.Events.OnAfterPipelineExceptionHandled(this, new PipelineExceptionEventArgs(pipelineEvent.Pipeline));
                }
            }
            finally
            {
                pipelineEvent.Pipeline.Abort();
            }
        }
    }
}