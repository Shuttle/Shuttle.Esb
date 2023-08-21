using System;
using System.IO;
using System.Threading.Tasks;
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

    public async ValueTask<bool> IsEmpty()
    {
        return await ValueTask.FromResult(false).ConfigureAwait(false);
    }

    public async Task Enqueue(TransportMessage message, Stream stream)
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task<ReceivedMessage> GetMessage()
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
            Message = await (await _serializer.Serialize(command)).ToBytesAsync().ConfigureAwait(false)
        };

        MessageCount += 1;

        return new ReceivedMessage(await _serializer.Serialize(transportMessage).ConfigureAwait(false), null);
    }

    public async Task Acknowledge(object acknowledgementToken)
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task Release(object acknowledgementToken)
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

    public event EventHandler<OperationEventArgs> OperationStarting = delegate
    {
    };

    public event EventHandler<OperationEventArgs> OperationCompleted = delegate
    {
    };
}