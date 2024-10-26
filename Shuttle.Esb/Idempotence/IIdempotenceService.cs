using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Shuttle.Esb;

public enum ProcessingStatus
{
    Active = 0,
    Ignore = 1,
    MessageHandled = 2,
    Assigned = 3
}

public interface IIdempotenceService
{
    ValueTask<bool> AddDeferredMessageAsync(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage, Stream deferredTransportMessageStream);
    Task DeferredMessageSentAsync(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage);
    Task<IEnumerable<Stream>> GetDeferredMessagesAsync(TransportMessage transportMessage);
    Task MessageHandledAsync(TransportMessage transportMessage);
    Task ProcessingCompletedAsync(TransportMessage transportMessage);
    ValueTask<ProcessingStatus> ProcessingStatusAsync(TransportMessage transportMessage);
}