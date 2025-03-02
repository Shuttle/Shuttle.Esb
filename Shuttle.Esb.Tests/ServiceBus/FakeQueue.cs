using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;
using JsonSerializer = Shuttle.Core.Serialization.JsonSerializer;

namespace Shuttle.Esb.Tests;

public class FakeQueue : IQueue
{
    private readonly ISerializer _serializer = new JsonSerializer(Options.Create(new JsonSerializerOptions()));

    public FakeQueue(int messagesToReturn)
    {
        MessagesToReturn = messagesToReturn;
    }

    public int MessageCount { get; private set; }

    public int MessagesToReturn { get; }

    public QueueUri Uri { get; } = new(new Uri("fake://configuration/queue"));
    public bool IsStream => false;

    public async ValueTask<bool> IsEmptyAsync()
    {
        Operation?.Invoke(this, new("IsEmpty"));

        return await ValueTask.FromResult(MessageCount < MessagesToReturn).ConfigureAwait(false);
    }

    public async Task EnqueueAsync(TransportMessage transportMessage, Stream stream)
    {
        MessageEnqueued?.Invoke(this, new(transportMessage, stream));

        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task AcknowledgeAsync(object acknowledgementToken)
    {
        MessageAcknowledged?.Invoke(this, new(acknowledgementToken));

        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task ReleaseAsync(object acknowledgementToken)
    {
        MessageReleased?.Invoke(this, new(acknowledgementToken));

        await Task.CompletedTask.ConfigureAwait(false);
    }

    public event EventHandler<MessageEnqueuedEventArgs>? MessageEnqueued;
    public event EventHandler<MessageAcknowledgedEventArgs>? MessageAcknowledged;
    public event EventHandler<MessageReleasedEventArgs>? MessageReleased;
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    public event EventHandler<OperationEventArgs>? Operation;

    public async Task<ReceivedMessage?> GetMessageAsync()
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
            ExpiryDate = expired ? DateTimeOffset.Now.AddMilliseconds(-1) : DateTimeOffset.MaxValue,
            PrincipalIdentityName = "Identity",
            AssemblyQualifiedName = command.GetType().AssemblyQualifiedName!,
            Message = await (await _serializer.SerializeAsync(command)).ToBytesAsync().ConfigureAwait(false)
        };

        MessageCount += 1;

        var result = new ReceivedMessage(await _serializer.SerializeAsync(transportMessage).ConfigureAwait(false), string.Empty);

        MessageReceived?.Invoke(this, new(result));

        return result;
    }
}