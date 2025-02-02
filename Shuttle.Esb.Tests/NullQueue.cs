using System;
using System.IO;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Tests;

public class NullQueue : IQueue
{
    public NullQueue(string uri)
        : this(new Uri(uri))
    {
    }

    public NullQueue(Uri uri)
    {
        Uri = new(uri);
    }

    public QueueUri Uri { get; }
    public bool IsStream => false;

    public async ValueTask<bool> IsEmptyAsync()
    {
        Operation?.Invoke(this, new("IsEmpty"));

        return await Task.FromResult(true);
    }

    public async Task EnqueueAsync(TransportMessage transportMessage, Stream stream)
    {
        MessageEnqueued?.Invoke(this, new(transportMessage, stream));

        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task<ReceivedMessage?> GetMessageAsync()
    {
        MessageReceived?.Invoke(this, new(new(Stream.Null, "token")));

        return await Task.FromResult<ReceivedMessage?>(null).ConfigureAwait(false);
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
}