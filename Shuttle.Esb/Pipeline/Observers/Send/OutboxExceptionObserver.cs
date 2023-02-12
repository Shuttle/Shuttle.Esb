using System.Threading.Tasks;
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

        public async Task Execute(OnPipelineException pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            try
            {
                state.ResetWorking();

                if (pipelineEvent.Pipeline.ExceptionHandled)
                {
                    return;
                }

                try
                {
                    var receivedMessage = state.GetReceivedMessage();
                    var transportMessage = state.GetTransportMessage();
                    var workQueue = state.GetWorkQueue();
                    var errorQueue = state.GetErrorQueue();

                    if (transportMessage == null)
                    {
                        if (receivedMessage != null)
                        {
                            await workQueue.Release(receivedMessage.AcknowledgementToken).ConfigureAwait(false);
                        }

                        return;
                    }

                    if (!workQueue.IsStream)
                    {
                        var action = _policy.EvaluateOutboxFailure(pipelineEvent);

                        transportMessage.RegisterFailure(pipelineEvent.Pipeline.Exception.AllMessages(),
                            action.TimeSpanToIgnoreRetriedMessage);

                        if (action.Retry || errorQueue == null)
                        {
                            await workQueue.Enqueue(transportMessage, await _serializer.Serialize(transportMessage).ConfigureAwait(false));
                        }
                        else
                        {
                            await errorQueue.Enqueue(transportMessage, await _serializer.Serialize(transportMessage)).ConfigureAwait(false);
                        }

                        await workQueue.Acknowledge(receivedMessage.AcknowledgementToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await workQueue.Release(receivedMessage.AcknowledgementToken).ConfigureAwait(false);
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