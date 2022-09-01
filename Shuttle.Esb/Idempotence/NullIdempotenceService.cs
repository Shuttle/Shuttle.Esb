using System;
using System.Collections.Generic;
using System.IO;

namespace Shuttle.Esb
{
    public class NullIdempotenceService : IIdempotenceService
    {
        public ProcessingStatus ProcessingStatus(TransportMessage transportMessage)
        {
            return Esb.ProcessingStatus.Active;
        }

        public void ProcessingCompleted(TransportMessage transportMessage)
        {
        }

        public bool AddDeferredMessage(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage, Stream deferredTransportMessageStream)
        {
            return false;
        }

        public IEnumerable<Stream> GetDeferredMessages(TransportMessage transportMessage)
        {
            return Array.Empty<Stream>();
        }

        public void DeferredMessageSent(TransportMessage processingTransportMessage,
            TransportMessage deferredTransportMessage)
        {
        }

        public void MessageHandled(TransportMessage transportMessage)
        {
        }
    }
}