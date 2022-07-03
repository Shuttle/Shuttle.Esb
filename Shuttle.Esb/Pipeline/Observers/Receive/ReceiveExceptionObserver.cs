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
                    var workQueue = state.GetWorkQueue();
                    var errorQueue = state.GetErrorQueue();
                    var handlerContext = (IHandlerContext)state.GetHandlerContext();
                    var isStream = workQueue is IStream;

                    if (transportMessage == null)
                    {
                        if (receivedMessage != null)
                        {
                            workQueue.Release(receivedMessage.AcknowledgementToken);
                        }

                        return;
                    }

                    var action = _policy.EvaluateMessageHandlingFailure(pipelineEvent);

                    transportMessage.RegisterFailure(
                        pipelineEvent.Pipeline.Exception.AllMessages(),
                        action.TimeSpanToIgnoreRetriedMessage);

                    var retry = !pipelineEvent.Pipeline.Exception.Contains<UnrecoverableHandlerException>()
                                &&
                                action.Retry
                                &&
                                handlerContext.ExceptionHandler.ShouldRetry
                                && !isStream;

                    var poison = !retry && errorQueue != null &&
                                 (
                                     !isStream ||
                                     !handlerContext.ExceptionHandler.ShouldBlock
                                 );

                    if (retry || poison)
                    {
                        using (var stream = _serializer.Serialize(transportMessage))
                        {
                            if (retry)
                            {
                                workQueue.Enqueue(transportMessage, stream);
                            }

                            if (poison)
                            {
                                errorQueue.Enqueue(transportMessage, stream);
                            }
                        }

                        workQueue.Acknowledge(receivedMessage.AcknowledgementToken);
                    }
                    else
                    {
                        workQueue.Release(receivedMessage.AcknowledgementToken);
                    }
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