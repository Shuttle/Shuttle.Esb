using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Shuttle.Esb;

public class NullIdempotenceService : IIdempotenceService
{
    public async ValueTask<ProcessingStatus> ProcessingStatusAsync(TransportMessage transportMessage)
    {
        return await new ValueTask<ProcessingStatus>(ProcessingStatus.Active);
    }

    public async Task ProcessingCompletedAsync(TransportMessage transportMessage)
    {
        await Task.CompletedTask;
    }

    public async ValueTask<bool> AddDeferredMessageAsync(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage, Stream deferredTransportMessageStream)
    {
        return await new ValueTask<bool>(false);
    }

    public async Task<IEnumerable<Stream>> GetDeferredMessagesAsync(TransportMessage transportMessage)
    {
        return await Task.FromResult(Array.Empty<Stream>());
    }

    public async Task DeferredMessageSentAsync(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage)
    {
        await Task.CompletedTask;
    }

    public async Task MessageHandledAsync(TransportMessage transportMessage)
    {
        await Task.CompletedTask;
    }
}