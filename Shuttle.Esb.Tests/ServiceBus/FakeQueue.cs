using System;
using System.IO;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;

namespace Shuttle.Esb.Tests;

public class FakeQueue : IQueue
{
    private readonly ISerializer _serializer = new DefaultSerializer();

    public FakeQueue(int messagesToReturn)
    {
        MessagesToReturn = messagesToReturn;
    }

    public int MessageCount { get; private set; }

    public int MessagesToReturn { get; }

    public QueueUri Uri { get; }
    public bool IsStream => false;

    public bool IsEmpty()
    {
        return MessageCount < MessagesToReturn;
    }

    public async ValueTask<bool> IsEmptyAsync()
    {
        return await ValueTask.FromResult(IsEmpty()).ConfigureAwait(false);
    }

    public void Enqueue(TransportMessage transportMessage, Stream stream)
    {
    }

    public async Task EnqueueAsync(TransportMessage transportMessage, Stream stream)
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }

    public ReceivedMessage GetMessage()
    {
        return GetMessageAsync(true).GetAwaiter().GetResult();
    }

    public async Task<ReceivedMessage> GetMessageAsync()
    {
        return await GetMessageAsync(false);
    }

    public void Acknowledge(object acknowledgementToken)
    {
    }

    public async Task AcknowledgeAsync(object acknowledgementToken)
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }

    public void Release(object acknowledgementToken)
    {
    }

    public async Task ReleaseAsync(object acknowledgementToken)
    {
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

    private async Task<ReceivedMessage> GetMessageAsync(bool sync)
    {
        if (MessageCount == MessagesToReturn)
        {
            return null;
        }

        var expired = MessageCount % 2 != 0;

        var command = new SimpleCommand(expired ? "Expired" : "HasNotExpired");

        var transportMessage = new TransportMessage
        {
            MessageType = command.GetType().Name,
            ExpiryDate = expired ? DateTime.Now.AddMilliseconds(-1) : DateTime.MaxValue,
            PrincipalIdentityName = "Identity",
            AssemblyQualifiedName = command.GetType().AssemblyQualifiedName,
            Message = sync
                ? _serializer.Serialize(command).ToBytes()
                : await (await _serializer.SerializeAsync(command)).ToBytesAsync().ConfigureAwait(false)
        };

        MessageCount += 1;

        return new ReceivedMessage(sync ? _serializer.Serialize(transportMessage) : await _serializer.SerializeAsync(transportMessage).ConfigureAwait(false), null);
    }
}