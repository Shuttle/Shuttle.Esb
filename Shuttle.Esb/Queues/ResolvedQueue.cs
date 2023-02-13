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
            Guard.AgainstNull(queue, nameof(queue));
            Guard.AgainstNull(uri, nameof(uri));

            _queue = queue;
            Uri = new QueueUri(uri);
            IsStream = queue.IsStream;
        }

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