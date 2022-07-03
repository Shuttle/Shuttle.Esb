using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb
{
    public interface IOutboxExceptionObserver : IPipelineObserver<OnPipelineException>
    {
    }

    public class OutboxExceptionObserver : IOutboxExceptionObserver
    {
        private readonly IServiceBusPolicy _policy;
        private readonly ISerializer _serializer;

        public OutboxExceptionObserver(IServiceBusPolicy policy, ISerializer serializer)
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
                    var receivedMessage = state.GetReceivedMessage();
                    var transportMessage = state.GetTransportMessage();
                    var workQueue = state.GetWorkQueue();

                    if (transportMessage == null)
                    {
                        if (receivedMessage != null)
                        {
                            workQueue.Release(receivedMessage.AcknowledgementToken);
                        }

                        return;
                    }

                    if (!workQueue.IsStream)
                    {
                        var action = _policy.EvaluateOutboxFailure(pipelineEvent);

                        transportMessage.RegisterFailure(pipelineEvent.Pipeline.Exception.AllMessages(),
                            action.TimeSpanToIgnoreRetriedMessage);

                        if (action.Retry)
                        {
                            workQueue.Enqueue(transportMessage, _serializer.Serialize(transportMessage));
                        }
                        else
                        {
                            state.GetErrorQueue().Enqueue(transportMessage, _serializer.Serialize(transportMessage));
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