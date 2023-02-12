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
                    var transportMessage = state.GetTransportMessage();
                    var receivedMessage = state.GetReceivedMessage();
                    var workQueue = state.GetWorkQueue();
                    var errorQueue = state.GetErrorQueue();
                    var handlerContext = (IHandlerContext)state.GetHandlerContext();

                    if (transportMessage == null)
                    {
                        if (receivedMessage != null)
                        {
                            await workQueue.Release(receivedMessage.AcknowledgementToken).ConfigureAwait(false);
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
                        await using (var stream = await _serializer.Serialize(transportMessage).ConfigureAwait(false))
                        {
                            if (retry)
                            {
                                await workQueue.Enqueue(transportMessage, stream).ConfigureAwait(false);
                            }

                            if (poison)
                            {
                                await errorQueue.Enqueue(transportMessage, stream).ConfigureAwait(false);
                            }
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