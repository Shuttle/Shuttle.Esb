using System;
using System.IO;
using System.Threading.Tasks;

namespace Shuttle.Esb.Tests;

public class NullQueue : IQueue
{
    public NullQueue(string uri)
        : this(new Uri(uri))
    {
    }

    public NullQueue(Uri uri)
    {
        Uri = new QueueUri(uri);
    }

    public QueueUri Uri { get; }
    public bool IsStream => false;

    public bool IsEmpty()
    {
        return true;
    }

    public async ValueTask<bool> IsEmptyAsync()
    {
        return await Task.FromResult(IsEmpty());
    }

    public void Enqueue(TransportMessage message, Stream stream)
    {
    }

    public async Task EnqueueAsync(TransportMessage transportMessage, Stream stream)
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }

    public ReceivedMessage GetMessage()
    {
        return null;
    }

    public async Task<ReceivedMessage> GetMessageAsync()
    {
        return await Task.FromResult<ReceivedMessage>(null).ConfigureAwait(false);
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

    public event EventHandler<OperationEventArgs> OperationStarting = delegate
    {
    };

    public event EventHandler<OperationEventArgs> OperationCompleted = delegate
    {
    };
}