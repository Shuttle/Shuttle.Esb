using System.Threading.Tasks;
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

        private async Task ExecuteAsync(OnPipelineException pipelineEvent, bool sync)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

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
                    var transportMessage = state.GetTransportMessage();
                    var receivedMessage = state.GetReceivedMessage();
                    var workQueue = state.GetWorkQueue();
                    var errorQueue = state.GetErrorQueue();
                    var handlerContext = (IHandlerContext)state.GetHandlerContext();

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

                    var action = _policy.EvaluateMessageHandlingFailure(pipelineEvent);

                    transportMessage.RegisterFailure(
                        pipelineEvent.Pipeline.Exception.AllMessages(),
                        action.TimeSpanToIgnoreRetriedMessage);

                    var retry = !workQueue.IsStream;

                    retry = retry && !pipelineEvent.Pipeline.Exception.Contains<UnrecoverableHandlerException>();
                    retry = retry && action.Retry;

                    if (retry && handlerContext != null)
                    {
                        retry =
                            handlerContext.ExceptionHandling == ExceptionHandling.Retry ||
                            handlerContext.ExceptionHandling == ExceptionHandling.Default;
                    }

                    var poison = errorQueue != null;

                    poison = poison && !retry;

                    if (poison && handlerContext != null)
                    {
                        poison =
                            handlerContext.ExceptionHandling == ExceptionHandling.Poison ||
                            handlerContext.ExceptionHandling == ExceptionHandling.Default;
                    }

                    if (retry || poison)
                    {
                        if (sync)
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

                            await workQueue.AcknowledgeAsync(receivedMessage.AcknowledgementToken).ConfigureAwait(false);
                        }
                        else
                        {
                            await using (var stream = await _serializer.SerializeAsync(transportMessage).ConfigureAwait(false))
                            {
                                if (retry)
                                {
                                    await workQueue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);
                                }

                                if (poison)
                                {
                                    await errorQueue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);
                                }
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