﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Streams;

namespace Shuttle.Esb.Tests;

public class MemoryQueue : IQueue
{
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

    public async ValueTask<bool> IsEmpty()
    {
        bool isEmpty;

        lock (_lock)
        {
            isEmpty = _queue.Count == 0;
        }

        return await ValueTask.FromResult(isEmpty).ConfigureAwait(false);
    }

    public async Task Enqueue(TransportMessage message, Stream stream)
    {
        var copy = await stream.CopyAsync().ConfigureAwait(false);

        lock (_lock)
        {
            _queue.Enqueue(new Message(message, copy));
        }
    }

    public async Task<ReceivedMessage> GetMessage()
    {
        Message message;

        lock (_lock)
        {
            if (_queue.Count == 0)
            {
                return null;
            }

            message = _queue.Dequeue();

            _unacked.Add(message.TransportMessage.MessageId, message);
        }

        return await Task.FromResult(new ReceivedMessage(message.Stream, message.TransportMessage.MessageId)).ConfigureAwait(false);
    }

    public async Task Acknowledge(object acknowledgementToken)
    {
        lock (_lock)
        {
            _unacked.Remove((Guid)acknowledgementToken);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task Release(object acknowledgementToken)
    {
        lock (_lock)
        {
            var token = (Guid)acknowledgementToken;

            _queue.Enqueue(_unacked[token]);
            _unacked.Remove(token);
        }

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

    public event EventHandler<OperationEventArgs> OperationStarting = delegate
    {
    };

    public event EventHandler<OperationEventArgs> OperationCompleted = delegate
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