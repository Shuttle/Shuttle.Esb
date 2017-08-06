using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class OutboxExceptionObserver : IPipelineObserver<OnPipelineException>
    {
        private readonly IServiceBusEvents _events;
        private readonly IServiceBusPolicy _policy;
        private readonly ISerializer _serializer;

        public OutboxExceptionObserver(IServiceBusEvents events, IServiceBusPolicy policy, ISerializer serializer)
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
                    var receivedMessage = state.GetReceivedMessage();
                    var transportMessage = state.GetTransportMessage();

                    if (transportMessage == null)
                    {
                        if (receivedMessage != null)
                        {
                            state.GetWorkQueue().Release(receivedMessage.AcknowledgementToken);
                        }

                        return;
                    }

                    var action = _policy.EvaluateOutboxFailure(pipelineEvent);

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

                    state.GetWorkQueue().Acknowledge(receivedMessage.AcknowledgementToken);
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