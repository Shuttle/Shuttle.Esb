using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb
{
    public interface IDistributorExceptionObserver : IPipelineObserver<OnPipelineException>
    {
    }

    public class DistributorExceptionObserver : IDistributorExceptionObserver
    {
        private readonly IServiceBusPolicy _policy;
        private readonly ISerializer _serializer;

        public DistributorExceptionObserver(IServiceBusPolicy policy, ISerializer serializer)
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

                    if (transportMessage == null)
                    {
                        return;
                    }

                    var workQueue = state.GetWorkQueue();
                    var errorQueue = state.GetErrorQueue();
                    var receivedMessage = state.GetReceivedMessage();

                    if (!workQueue.IsStream)
                    {
                        var action = _policy.EvaluateMessageDistributionFailure(pipelineEvent);

                        transportMessage.RegisterFailure(pipelineEvent.Pipeline.Exception.AllMessages(),
                            action.TimeSpanToIgnoreRetriedMessage);

                        if (action.Retry || errorQueue == null)
                        {
                            await workQueue.Enqueue(transportMessage, await _serializer.Serialize(transportMessage).ConfigureAwait(false)).ConfigureAwait(false);
                        }
                        else
                        {
                            await errorQueue.Enqueue(transportMessage, await _serializer.Serialize(transportMessage).ConfigureAwait(false)).ConfigureAwait(false);
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