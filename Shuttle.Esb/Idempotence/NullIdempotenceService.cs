using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public class NullIdempotenceService : IIdempotenceService
    {
        public ProcessingStatus ProcessingStatus(TransportMessage transportMessage)
        {
            return Esb.ProcessingStatus.Active;
        }

        public async ValueTask<ProcessingStatus> ProcessingStatusAsync(TransportMessage transportMessage)
        {
            return await new ValueTask<ProcessingStatus>(Esb.ProcessingStatus.Active);
        }

        public void ProcessingCompleted(TransportMessage transportMessage)
        {
        }

        public async Task ProcessingCompletedAsync(TransportMessage transportMessage)
        {
            await Task.CompletedTask;
        }

        public bool AddDeferredMessage(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage, Stream deferredTransportMessageStream)
        {
            return false;
        }

        public async ValueTask<bool> AddDeferredMessageAsync(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage, Stream deferredTransportMessageStream)
        {
            return await new ValueTask<bool>(false);
        }

        public IEnumerable<Stream> GetDeferredMessages(TransportMessage transportMessage)
        {
            return Array.Empty<Stream>();
        }

        public async Task<IEnumerable<Stream>> GetDeferredMessagesAsync(TransportMessage transportMessage)
        {
            return await Task.FromResult(Array.Empty<Stream>());
        }

        public void DeferredMessageSent(TransportMessage processingTransportMessage,
            TransportMessage deferredTransportMessage)
        {
        }

        public async Task DeferredMessageSentAsync(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage)
        {
            await Task.CompletedTask;
        }

        public void MessageHandled(TransportMessage transportMessage)
        {
        }

        public async Task MessageHandledAsync(TransportMessage transportMessage)
        {
            await Task.CompletedTask;
        }
    }
}