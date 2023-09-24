using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public enum ProcessingStatus
    {
        Active = 0,
        Ignore = 1,
        MessageHandled = 2,
        Assigned = 3
    }

    public interface IIdempotenceService
    {
        ProcessingStatus ProcessingStatus(TransportMessage transportMessage);
        ValueTask<ProcessingStatus> ProcessingStatusAsync(TransportMessage transportMessage);
        void ProcessingCompleted(TransportMessage transportMessage);
        Task ProcessingCompletedAsync(TransportMessage transportMessage);
        bool AddDeferredMessage(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage, Stream deferredTransportMessageStream);
        ValueTask<bool> AddDeferredMessageAsync(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage, Stream deferredTransportMessageStream);
        IEnumerable<Stream> GetDeferredMessages(TransportMessage transportMessage);
        Task<IEnumerable<Stream>> GetDeferredMessagesAsync(TransportMessage transportMessage);
        void DeferredMessageSent(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage);
        Task DeferredMessageSentAsync(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage);
        void MessageHandled(TransportMessage transportMessage);
        Task MessageHandledAsync(TransportMessage transportMessage);
    }
}