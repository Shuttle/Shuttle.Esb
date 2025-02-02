using System;
using System.IO;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class ResolvedQueue : IQueue
{
    private readonly IQueue _queue;

    public ResolvedQueue(IQueue queue, Uri uri)
    {
        _queue = Guard.AgainstNull(queue);
        Uri = new(Guard.AgainstNull(uri));
        IsStream = queue.IsStream;

        _queue.MessageAcknowledged += (sender, args) =>
        {
            MessageAcknowledged?.Invoke(sender, args);
        };

        _queue.MessageEnqueued += (sender, args) =>
        {
            MessageEnqueued?.Invoke(sender, args);
        };

        _queue.MessageReceived += (sender, args) =>
        {
            MessageReceived?.Invoke(sender, args);
        };

        _queue.MessageReleased += (sender, args) =>
        {
            MessageReleased?.Invoke(sender, args);
        };

        _queue.Operation += (sender, args) =>
        {
            Operation?.Invoke(sender, args);
        };
    }

    public event EventHandler<MessageEnqueuedEventArgs>? MessageEnqueued;
    public event EventHandler<MessageAcknowledgedEventArgs>? MessageAcknowledged;
    public event EventHandler<MessageReleasedEventArgs>? MessageReleased;
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    public event EventHandler<OperationEventArgs>? Operation;

    public QueueUri Uri { get; }
    public bool IsStream { get; }

    public async ValueTask<bool> IsEmptyAsync()
    {
        return await _queue.IsEmptyAsync();
    }

    public async Task EnqueueAsync(TransportMessage transportMessage, Stream stream)
    {
        await _queue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);
    }

    public async Task<ReceivedMessage?> GetMessageAsync()
    {
        return await _queue.GetMessageAsync().ConfigureAwait(false);
    }

    public async Task AcknowledgeAsync(object acknowledgementToken)
    {
        await _queue.AcknowledgeAsync(acknowledgementToken).ConfigureAwait(false);
    }

    public async Task ReleaseAsync(object acknowledgementToken)
    {
        await _queue.ReleaseAsync(acknowledgementToken).ConfigureAwait(false);
    }
}