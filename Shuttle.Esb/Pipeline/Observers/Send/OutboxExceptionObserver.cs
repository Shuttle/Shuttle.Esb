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

        private async Task ExecuteAsync(OnPipelineException pipelineEvent, bool sync)
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
                        if (receivedMessage == null)
                        {
                            return;
                        }

                        if (sync)
                        {
                            workQueue.Release(receivedMessage.AcknowledgementToken);
                        }
                        else
                        {
                            await workQueue.ReleaseAsync(receivedMessage.AcknowledgementToken).ConfigureAwait(false);
                        }

                        return;
                    }

                    if (!workQueue.IsStream)
                    {
                        var action = _policy.EvaluateOutboxFailure(pipelineEvent);

                        transportMessage.RegisterFailure(pipelineEvent.Pipeline.Exception.AllMessages(),
                            action.TimeSpanToIgnoreRetriedMessage);

                        if (sync)
                        {
                            if (action.Retry || errorQueue == null)
                            {
                                workQueue.Enqueue(transportMessage, _serializer.Serialize(transportMessage));
                            }
                            else
                            {
                                errorQueue.Enqueue(transportMessage, _serializer.Serialize(transportMessage));
                            }

                            workQueue.Acknowledge(receivedMessage.AcknowledgementToken);
                        }
                        else
                        {
                            if (action.Retry || errorQueue == null)
                            {
                                await workQueue.EnqueueAsync(transportMessage, await _serializer.SerializeAsync(transportMessage).ConfigureAwait(false)).ConfigureAwait(false);
                            }
                            else
                            {
                                await errorQueue.EnqueueAsync(transportMessage, await _serializer.SerializeAsync(transportMessage).ConfigureAwait(false)).ConfigureAwait(false);
                            }

                            await workQueue.AcknowledgeAsync(receivedMessage.AcknowledgementToken).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        if (sync)
                        {
                            workQueue.Release(receivedMessage.AcknowledgementToken);
                        }
                        else
                        {
                            await workQueue.ReleaseAsync(receivedMessage.AcknowledgementToken).ConfigureAwait(false);
                        }
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

        public void Execute(OnPipelineException pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnPipelineException pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }
    }
}