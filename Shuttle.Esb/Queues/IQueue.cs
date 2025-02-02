using System;
using System.IO;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public interface IQueue
{
    bool IsStream { get; }

    QueueUri Uri { get; }
    Task AcknowledgeAsync(object acknowledgementToken);
    Task EnqueueAsync(TransportMessage transportMessage, Stream stream);
    Task<ReceivedMessage?> GetMessageAsync();
    ValueTask<bool> IsEmptyAsync();
    Task ReleaseAsync(object acknowledgementToken);

    event EventHandler<MessageAcknowledgedEventArgs>? MessageAcknowledged;
    event EventHandler<MessageEnqueuedEventArgs>? MessageEnqueued;
    event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    event EventHandler<MessageReleasedEventArgs>? MessageReleased;
    event EventHandler<OperationEventArgs>? Operation;
}