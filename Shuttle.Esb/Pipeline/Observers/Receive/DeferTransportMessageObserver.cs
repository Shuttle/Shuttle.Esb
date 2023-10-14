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
        private readonly IDeferredMessageProcessor _deferredMessageProcessor;

        public DeferTransportMessageObserver(IDeferredMessageProcessor deferredMessageProcessor)
        {
            _deferredMessageProcessor = Guard.AgainstNull(deferredMessageProcessor, nameof(deferredMessageProcessor));
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
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var transportMessage = Guard.AgainstNull(state.GetTransportMessage(), StateKeys.TransportMessage);
            var workQueue = Guard.AgainstNull(state.GetWorkQueue(), StateKeys.WorkQueue);

            if (!transportMessage.IsIgnoring() || workQueue.IsStream)
            {
                return;
            }

            var receivedMessage = Guard.AgainstNull(state.GetReceivedMessage(), StateKeys.ReceivedMessage);
            var deferredQueue = state.GetDeferredQueue();

            if (sync)
            {
                using (var stream = receivedMessage.Stream.Copy())
                {
                    if (deferredQueue == null)
                    {
                        workQueue.Enqueue(transportMessage, stream);
                    }
                    else
                    {
                        deferredQueue.Enqueue(transportMessage, stream);

                        _deferredMessageProcessor.MessageDeferred(transportMessage.IgnoreTillDate);
                    }
                }

                workQueue.Acknowledge(receivedMessage.AcknowledgementToken);
            }
            else
            {
                await using (var stream = await receivedMessage.Stream.CopyAsync().ConfigureAwait(false))
                {
                    if (deferredQueue == null)
                    {
                        await workQueue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);
                    }
                    else
                    {
                        await deferredQueue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);

                        await _deferredMessageProcessor.MessageDeferredAsync(transportMessage.IgnoreTillDate).ConfigureAwait(false);
                    }
                }

                await workQueue.AcknowledgeAsync(receivedMessage.AcknowledgementToken).ConfigureAwait(false);
            }

            TransportMessageDeferred?.Invoke(this, new TransportMessageDeferredEventArgs(transportMessage));

            pipelineEvent.Pipeline.Abort();
        }

        public event EventHandler<TransportMessageDeferredEventArgs> TransportMessageDeferred;
    }
}