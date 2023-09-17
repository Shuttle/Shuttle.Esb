using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb
{
    public interface IDeferTransportMessageObserver : IPipelineObserver<OnAfterDeserializeTransportMessage>
    {
        event EventHandler<TransportMessageDeferredEventArgs> TransportMessageDeferred;
    }

    public class DeferTransportMessageObserver : IDeferTransportMessageObserver
    {
        private readonly IServiceBusConfiguration _serviceBusConfiguration;

        public DeferTransportMessageObserver(IServiceBusConfiguration serviceBusConfiguration)
        {
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));

            _serviceBusConfiguration = serviceBusConfiguration;
        }

        public void Execute(OnAfterDeserializeTransportMessage pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnAfterDeserializeTransportMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnAfterDeserializeTransportMessage pipelineEvent, bool sync)
        {
            var state = pipelineEvent.Pipeline.State;
            var receivedMessage = state.GetReceivedMessage();
            var transportMessage = state.GetTransportMessage();
            var workQueue = state.GetWorkQueue();

            Guard.AgainstNull(receivedMessage, nameof(receivedMessage));
            Guard.AgainstNull(transportMessage, nameof(transportMessage));
            Guard.AgainstNull(workQueue, nameof(workQueue));

            if (!transportMessage.IsIgnoring())
            {
                return;
            }

            if (sync)
            {
                using (var stream = receivedMessage.Stream.Copy())
                {
                    if (state.GetDeferredQueue() == null)
                    {
                        workQueue.Enqueue(transportMessage, stream);
                    }
                    else
                    {
                        state.GetDeferredQueue().Enqueue(transportMessage, stream);

                        _serviceBusConfiguration.Inbox.DeferredMessageProcessor.MessageDeferred(transportMessage.IgnoreTillDate);
                    }
                }

                workQueue.Acknowledge(receivedMessage.AcknowledgementToken);
            }
            else
            {
                await using (var stream = await receivedMessage.Stream.CopyAsync().ConfigureAwait(false))
                {
                    if (state.GetDeferredQueue() == null)
                    {
                        await workQueue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);
                    }
                    else
                    {
                        await state.GetDeferredQueue().EnqueueAsync(transportMessage, stream).ConfigureAwait(false);

                        await _serviceBusConfiguration.Inbox.DeferredMessageProcessor.MessageDeferredAsync(transportMessage.IgnoreTillDate).ConfigureAwait(false);
                    }
                }

                await workQueue.AcknowledgeAsync(receivedMessage.AcknowledgementToken).ConfigureAwait(false);
            }

            TransportMessageDeferred.Invoke(this, new TransportMessageDeferredEventArgs(transportMessage));

            pipelineEvent.Pipeline.Abort();
        }

        public event EventHandler<TransportMessageDeferredEventArgs> TransportMessageDeferred = delegate
        {
        };
    }
}