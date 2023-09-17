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

            _queue.OperationStarting += (sender, args) =>
            {
                OperationStarting.Invoke(sender, args);
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

        public event EventHandler<OperationEventArgs> OperationStarting = delegate
        {
        };

        public event EventHandler<OperationEventArgs> OperationCompleted = delegate
        {
        };

        public QueueUri Uri { get; }
        public bool IsStream { get; }

        public bool IsEmpty()
        {
            return _queue.IsEmpty();
        }

        public ValueTask<bool> IsEmptyAsync()
        {
            return _queue.IsEmptyAsync();
        }

        public void Enqueue(TransportMessage message, Stream stream)
        {
            _queue.Enqueue(message, stream);
        }

        public async Task EnqueueAsync(TransportMessage transportMessage, Stream stream)
        {
            await _queue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);
        }

        public ReceivedMessage GetMessage()
        {
            return _queue.GetMessage();
        }

        public async Task<ReceivedMessage> GetMessageAsync()
        {
            return await _queue.GetMessageAsync().ConfigureAwait(false);
        }

        public void Acknowledge(object acknowledgementToken)
        {
            _queue.Acknowledge(acknowledgementToken);
        }

        public async Task AcknowledgeAsync(object acknowledgementToken)
        {
            await _queue.AcknowledgeAsync(acknowledgementToken).ConfigureAwait(false);
        }

        public void Release(object acknowledgementToken)
        {
            _queue.Release(acknowledgementToken);
        }

        public async Task ReleaseAsync(object acknowledgementToken)
        {
            await _queue.ReleaseAsync(acknowledgementToken).ConfigureAwait(false);
        }
    }
}