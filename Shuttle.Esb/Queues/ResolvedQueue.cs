using System;
using System.IO;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ResolvedQueue : IQueue
    {
        private readonly IQueue _queue;

        public ResolvedQueue(IQueue queue, Uri uri)
        {
            
            _queue = Guard.AgainstNull(queue, nameof(queue));
            Uri = new QueueUri(Guard.AgainstNull(uri, nameof(uri)));
            IsStream = queue.IsStream;

            _queue.MessageAcknowledged += (sender, args) =>
            {
                MessageAcknowledged.Invoke(sender, args);
            };

            _queue.MessageEnqueued += (sender, args) =>
            {
                MessageEnqueued.Invoke(sender, args);
            };

            _queue.MessageReceived += (sender, args) =>
            {
                MessageReceived.Invoke(sender, args);
            };

            _queue.MessageReleased += (sender, args) =>
            {
                MessageReleased.Invoke(sender, args);
            };

            _queue.OperationCompleted += (sender, args) =>
            {
                OperationCompleted.Invoke(sender, args);
            };
        }

        public event EventHandler<MessageEnqueuedEventArgs> MessageEnqueued = delegate
        {
        };

        public event EventHandler<MessageAcknowledgedEventArgs> MessageAcknowledged = delegate
        {
        };

        public event EventHandler<MessageReleasedEventArgs> MessageReleased = delegate
        {
        };

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate
        {
        };

        public event EventHandler<OperationCompletedEventArgs> OperationCompleted = delegate
        {
        };

        public QueueUri Uri { get; }
        public bool IsStream { get; }

        public ValueTask<bool> IsEmpty()
        {
            return _queue.IsEmpty();
        }

        public async Task Enqueue(TransportMessage transportMessage, Stream stream)
        {
            await _queue.Enqueue(transportMessage, stream).ConfigureAwait(false);
        }

        public Task<ReceivedMessage> GetMessage()
        {
            return _queue.GetMessage();
        }

        public async Task Acknowledge(object acknowledgementToken)
        {
            await _queue.Acknowledge(acknowledgementToken).ConfigureAwait(false);
        }

        public async Task Release(object acknowledgementToken)
        {
            await _queue.Release(acknowledgementToken).ConfigureAwait(false);
        }
    }
}