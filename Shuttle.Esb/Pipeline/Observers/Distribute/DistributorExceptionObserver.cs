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
                    var transportMessage = state.GetTransportMessage();

                    if (transportMessage == null)
                    {
                        return;
                    }

                    var errorQueue = state.GetErrorQueue();
                    var workQueue = Guard.AgainstNull(state.GetWorkQueue(), "WorkQueue");
                    var receivedMessage = Guard.AgainstNull(state.GetReceivedMessage(), "ReceivedMessage");

                    if (!workQueue.IsStream)
                    {
                        var action = _policy.EvaluateMessageDistributionFailure(pipelineEvent);

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