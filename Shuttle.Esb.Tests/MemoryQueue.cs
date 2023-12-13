using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Streams;

namespace Shuttle.Esb.Tests;

public class MemoryQueue : IQueue
{
    private readonly object _lock = new();
    private readonly Queue<Message> _queue = new();
    private readonly Dictionary<Guid, Message> _unacknowledged = new();

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

    public async ValueTask<bool> IsEmptyAsync()
    {
        return await ValueTask.FromResult(IsEmpty()).ConfigureAwait(false);
    }

    public void Enqueue(TransportMessage transportMessage, Stream stream)
    {
        var copy = stream.Copy();

        lock (_lock)
        {
            _queue.Enqueue(new Message(transportMessage, copy));
        }
    }

    public async Task EnqueueAsync(TransportMessage transportMessage, Stream stream)
    {
        var copy = await stream.CopyAsync().ConfigureAwait(false);

        lock (_lock)
        {
            _queue.Enqueue(new Message(transportMessage, copy));
        }
    }

    public ReceivedMessage GetMessage()
    {
        Message message;

        lock (_lock)
        {
            if (_queue.Count == 0)
            {
                return null;
            }

            message = _queue.Dequeue();

            _unacknowledged.Add(message.TransportMessage.MessageId, message);
        }

        return new ReceivedMessage(message.Stream, message.TransportMessage.MessageId);
    }

    public async Task<ReceivedMessage> GetMessageAsync()
    {
        return await Task.FromResult(GetMessage()).ConfigureAwait(false);
    }

    public void Acknowledge(object acknowledgementToken)
    {
        lock (_lock)
        {
            _unacknowledged.Remove((Guid)acknowledgementToken);
        }
    }

    public async Task AcknowledgeAsync(object acknowledgementToken)
    {
        Acknowledge(acknowledgementToken);

        await Task.CompletedTask.ConfigureAwait(false);
    }

    public void Release(object acknowledgementToken)
    {
        lock (_lock)
        {
            var token = (Guid)acknowledgementToken;

            _queue.Enqueue(_unacknowledged[token]);
            _unacknowledged.Remove(token);
        }
    }

    public async Task ReleaseAsync(object acknowledgementToken)
    {
        Release(acknowledgementToken);

        await Task.CompletedTask.ConfigureAwait(false);
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

    public event EventHandler<OperationEventArgs> Operation = delegate
    {
    };

    private class Message
    {
        public Message(TransportMessage transportMessage, Stream stream)
        {
            TransportMessage = transportMessage;
            Stream = stream;
        }

        public Stream Stream { get; }
        public TransportMessage TransportMessage { get; }
    }
}