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

        public async Task Execute(OnAfterDeserializeTransportMessage pipelineEvent)
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

            using (var stream = await receivedMessage.Stream.CopyAsync().ConfigureAwait(false))
            {
                if (state.GetDeferredQueue() == null)
                {
                    await workQueue.Enqueue(transportMessage, stream).ConfigureAwait(false);
                }
                else
                {
                    await state.GetDeferredQueue().Enqueue(transportMessage, stream).ConfigureAwait(false);

                    _serviceBusConfiguration.Inbox.DeferredMessageProcessor.MessageDeferred(transportMessage.IgnoreTillDate);
                }
            }

            await workQueue.Acknowledge(receivedMessage.AcknowledgementToken).ConfigureAwait(false);

            TransportMessageDeferred.Invoke(this, new TransportMessageDeferredEventArgs(transportMessage));

            pipelineEvent.Pipeline.Abort();
        }

        public event EventHandler<TransportMessageDeferredEventArgs> TransportMessageDeferred = delegate
        {
        };
    }
}