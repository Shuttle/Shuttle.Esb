using System;
using System.Collections.Generic;
using System.IO;
using Shuttle.Core.Contract;
using Shuttle.Core.Streams;

namespace Shuttle.Esb.Tests
{
    public class MemoryQueue : IQueue
    {
        private class Message
        {
            public TransportMessage TransportMessage { get; }
            public Stream Stream { get; }

            public Message(TransportMessage transportMessage, Stream stream)
            {
                TransportMessage = transportMessage;
                Stream = stream;
            }
        }

        private readonly object _lock = new object();
        private readonly Queue<Message> _queue = new Queue<Message>();
        private readonly Dictionary<Guid, Message> _unacked = new Dictionary<Guid, Message>();

        public MemoryQueue(Uri uri)
        {
            Guard.AgainstNull(uri, nameof(uri));

            Uri = new QueueUri(uri);
        }

        public QueueUri Uri { get; }
        public bool IsStream => false;
        public bool IsEmpty()
        {
            lock (_lock)
            {
                return _queue.Count == 0;
            }
        }

        public void Enqueue(TransportMessage message, Stream stream)
        {
            lock (_lock)
            {
                _queue.Enqueue(new Message(message, stream.Copy()));
            }
        }

        public ReceivedMessage GetMessage()
        {
            lock (_lock)
            {
                if (_queue.Count == 0)
                {
                    return null;
                }

                var message = _queue.Dequeue();

                _unacked.Add(message.TransportMessage.MessageId, message);

                return new ReceivedMessage(message.Stream, message.TransportMessage.MessageId);
            }
        }

        public void Acknowledge(object acknowledgementToken)
        {
            lock (_lock)
            {
                _unacked.Remove((Guid)acknowledgementToken);
            }
        }

        public void Release(object acknowledgementToken)
        {
            lock (_lock)
            {
                var token = (Guid)acknowledgementToken;

                _queue.Enqueue(_unacked[token]);
                _unacked.Remove(token);
            }
        }
    }
}