using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb
{
    public interface IReceiveExceptionObserver : IPipelineObserver<OnPipelineException>
    {
    }

    public class ReceiveExceptionObserver : IReceiveExceptionObserver
    {
        private readonly IServiceBusPolicy _policy;
        private readonly ISerializer _serializer;

        public ReceiveExceptionObserver(IServiceBusPolicy policy, ISerializer serializer)
        {
            Guard.AgainstNull(policy, nameof(policy));
            Guard.AgainstNull(serializer, nameof(serializer));

            _policy = policy;
            _serializer = serializer;
        }

        public void Execute(OnPipelineException pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            try
            {
                if (pipelineEvent.Pipeline.ExceptionHandled)
                {
                    return;
                }

                try
                {
                    var transportMessage = state.GetTransportMessage();
                    var receivedMessage = state.GetReceivedMessage();

                    if (transportMessage == null)
                    {
                        if (receivedMessage != null)
                        {
                            state.GetWorkQueue().Release(receivedMessage.AcknowledgementToken);
                        }

                        return;
                    }

                    var action = _policy.EvaluateMessageHandlingFailure(pipelineEvent);

                    transportMessage.RegisterFailure(
                        pipelineEvent.Pipeline.Exception.AllMessages(),
                        action.TimeSpanToIgnoreRetriedMessage);

                    using (var stream = _serializer.Serialize(transportMessage))
                    {
                        var retry = !pipelineEvent.Pipeline.Exception.Contains<UnrecoverableHandlerException>()
                                    &&
                                    action.Retry;

                        if (retry)
                        {
                            state.GetWorkQueue().Enqueue(transportMessage, stream);
                        }
                        else
                        {
                            state.GetErrorQueue().Enqueue(transportMessage, stream);
                        }
                    }

                    state.GetWorkQueue().Acknowledge(receivedMessage.AcknowledgementToken);
                }
                finally
                {
                    pipelineEvent.Pipeline.MarkExceptionHandled();
                }
            }
            finally
            {
                pipelineEvent.Pipeline.Abort();
            }
        }
    }
}