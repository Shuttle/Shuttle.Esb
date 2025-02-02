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
        Uri = new(Guard.AgainstNull(uri));
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
        Operation?.Invoke(this, new("IsEmpty"));

        return await ValueTask.FromResult(IsEmpty()).ConfigureAwait(false);
    }

    public async Task EnqueueAsync(TransportMessage transportMessage, Stream stream)
    {
        var copy = await stream.CopyAsync().ConfigureAwait(false);

        lock (_lock)
        {
            _queue.Enqueue(new(transportMessage, copy));
        }

        MessageEnqueued?.Invoke(this, new(transportMessage, copy));
    }

    public async Task<ReceivedMessage?> GetMessageAsync()
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

        var result = await Task.FromResult(new ReceivedMessage(message.Stream, message.TransportMessage.MessageId)).ConfigureAwait(false);

        MessageReceived?.Invoke(this, new(result));

        return result;
    }

    public async Task AcknowledgeAsync(object acknowledgementToken)
    {
        lock (_lock)
        {
            _unacknowledged.Remove((Guid)acknowledgementToken);
        }

        MessageAcknowledged?.Invoke(this, new(acknowledgementToken));

        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task ReleaseAsync(object acknowledgementToken)
    {
        lock (_lock)
        {
            var token = (Guid)acknowledgementToken;

            _queue.Enqueue(_unacknowledged[token]);
            _unacknowledged.Remove(token);
        }

        MessageReleased?.Invoke(this, new(acknowledgementToken));

        await Task.CompletedTask.ConfigureAwait(false);
    }

    public event EventHandler<MessageEnqueuedEventArgs>? MessageEnqueued;
    public event EventHandler<MessageAcknowledgedEventArgs>? MessageAcknowledged;
    public event EventHandler<MessageReleasedEventArgs>? MessageReleased;
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    public event EventHandler<OperationEventArgs>? Operation;

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