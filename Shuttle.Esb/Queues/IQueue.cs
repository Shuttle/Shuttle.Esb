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
        bool IsEmpty();
        ValueTask<bool> IsEmptyAsync();
        void Enqueue(TransportMessage message, Stream stream);
        Task EnqueueAsync(TransportMessage message, Stream stream);
        ReceivedMessage GetMessage();
        Task<ReceivedMessage> GetMessageAsync();
        void Acknowledge(object acknowledgementToken);
        Task AcknowledgeAsync(object acknowledgementToken);
        void Release(object acknowledgementToken);
        Task ReleaseAsync(object acknowledgementToken);
    }
}