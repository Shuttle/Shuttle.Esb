using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb;

public interface IOutboxExceptionObserver : IPipelineObserver<OnPipelineException>
{
}

public class OutboxExceptionObserver : IOutboxExceptionObserver
{
    private readonly IServiceBusPolicy _policy;
    private readonly ISerializer _serializer;

    public OutboxExceptionObserver(IServiceBusPolicy policy, ISerializer serializer)
    {
        _policy = Guard.AgainstNull(policy);
        _serializer = Guard.AgainstNull(serializer);
    }

    public async Task ExecuteAsync(IPipelineContext<OnPipelineException> pipelineContext)
    {
        var state = pipelineContext.Pipeline.State;

        try
        {
            state.ResetWorking();

            if (pipelineContext.Pipeline.ExceptionHandled)
            {
                return;
            }

            try
            {
                var receivedMessage = state.GetReceivedMessage();
                var transportMessage = state.GetTransportMessage();
                var workQueue = Guard.AgainstNull(state.GetWorkQueue());
                var errorQueue = state.GetErrorQueue();

                if (transportMessage == null)
                {
                    if (receivedMessage == null)
                    {
                        return;
                    }

                    await workQueue.ReleaseAsync(receivedMessage.AcknowledgementToken).ConfigureAwait(false);

                    return;
                }

                Guard.AgainstNull(receivedMessage);

                if (!workQueue.IsStream)
                {
                    var action = _policy.EvaluateOutboxFailure(pipelineContext);

                    transportMessage.RegisterFailure(Guard.AgainstNull(pipelineContext.Pipeline.Exception).AllMessages(), action.TimeSpanToIgnoreRetriedMessage);

                    if (action.Retry || errorQueue == null)
                    {
                        await workQueue.EnqueueAsync(transportMessage, await _serializer.SerializeAsync(transportMessage).ConfigureAwait(false)).ConfigureAwait(false);
                    }
                    else
                    {
                        await errorQueue.EnqueueAsync(transportMessage, await _serializer.SerializeAsync(transportMessage).ConfigureAwait(false)).ConfigureAwait(false);
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
            }
        }
        finally
        {
            pipelineContext.Pipeline.Abort();
        }
    }
}