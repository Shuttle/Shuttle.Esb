using System;
using System.IO;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public interface IQueue
    {
        event EventHandler<MessageEnqueuedEventArgs> MessageEnqueued;
        event EventHandler<MessageAcknowledgedEventArgs> MessageAcknowledged;
        event EventHandler<MessageReleasedEventArgs> MessageReleased;
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        event EventHandler<OperationEventArgs> Operation;

        QueueUri Uri { get; }
        bool IsStream { get; }
        bool IsEmpty();
        ValueTask<bool> IsEmptyAsync();
        void Enqueue(TransportMessage transportMessage, Stream stream);
        Task EnqueueAsync(TransportMessage transportMessage, Stream stream);
        ReceivedMessage GetMessage();
        Task<ReceivedMessage> GetMessageAsync();
        void Acknowledge(object acknowledgementToken);
        Task AcknowledgeAsync(object acknowledgementToken);
        void Release(object acknowledgementToken);
        Task ReleaseAsync(object acknowledgementToken);
    }
}