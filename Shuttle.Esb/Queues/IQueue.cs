using System;
using System.IO;
using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public interface IQueue
    {
        event EventHandler<MessageEnqueuedEventArgs> MessageEnqueued;
        event EventHandler<MessageAcknowledgedEventArgs> MessageAcknowledged;
        event EventHandler<MessageReleasedEventArgs> MessageReleased;
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        event EventHandler<OperationEventArgs> OperationStarting;
        event EventHandler<OperationEventArgs> OperationCompleted;

        QueueUri Uri { get; }
        bool IsStream { get; }
        ValueTask<bool> IsEmpty();
        Task Enqueue(TransportMessage message, Stream stream);
        Task<ReceivedMessage> GetMessage();
        Task Acknowledge(object acknowledgementToken);
        Task Release(object acknowledgementToken);
    }
}