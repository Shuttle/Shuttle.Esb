using System.Threading.Tasks;
using System.Transactions;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb;

public interface IReceiveExceptionObserver : IPipelineObserver<OnPipelineException>
{
}

public class ReceiveExceptionObserver : IReceiveExceptionObserver
{
    private readonly IServiceBusPolicy _policy;
    private readonly ISerializer _serializer;

    public ReceiveExceptionObserver(IServiceBusPolicy policy, ISerializer serializer)
    {
        _policy = Guard.AgainstNull(policy);
        _serializer = Guard.AgainstNull(serializer);
    }

    public async Task ExecuteAsync(IPipelineContext<OnPipelineException> pipelineContext)
    {
        Guard.AgainstNull(pipelineContext);

        var state = pipelineContext.Pipeline.State;

        try
        {
            using (var tx = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
            {
                state.ResetWorking();

                if (pipelineContext.Pipeline.ExceptionHandled)
                {
                    return;
                }

                try
                {
                    var transportMessage = state.GetTransportMessage();
                    var receivedMessage = state.GetReceivedMessage();
                    var workQueue = Guard.AgainstNull(state.GetWorkQueue());

                    if (transportMessage == null)
                    {
                        if (receivedMessage == null)
                        {
                            return;
                        }

                        await workQueue.ReleaseAsync(receivedMessage.AcknowledgementToken).ConfigureAwait(false);

                        return;
                    }

                    var action = _policy.EvaluateMessageHandlingFailure(pipelineContext);

                    var errorQueue = state.GetErrorQueue();
                    var handlerContext = state.GetHandlerContext() as IHandlerContext;
                    var exception = Guard.AgainstNull(pipelineContext.Pipeline.Exception);

                    transportMessage.RegisterFailure(exception.AllMessages(), action.TimeSpanToIgnoreRetriedMessage);

                    var retry = !workQueue.IsStream;

                    retry = retry && !exception.Contains<UnrecoverableHandlerException>();
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

                    Guard.AgainstNull(receivedMessage);

                    if (retry || poison)
                    {
                        await using (var stream = await _serializer.SerializeAsync(transportMessage).ConfigureAwait(false))
                        {
                            if (retry)
                            {
                                await workQueue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);
                            }

                            if (poison)
                            {
                                await errorQueue!.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);
                            }
                        }

                        await workQueue.AcknowledgeAsync(receivedMessage!.AcknowledgementToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await workQueue.ReleaseAsync(receivedMessage!.AcknowledgementToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    pipelineContext.Pipeline.MarkExceptionHandled();

                    tx.Complete();
                }
            }
        }
        finally
        {
            pipelineContext.Pipeline.Abort();
        }
    }
}