using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Shuttle.Esb
{
    public class NullIdempotenceService : IIdempotenceService
    {
        private readonly List<Stream> _empty = new List<Stream>();

        public ProcessingStatus ProcessingStatus(TransportMessage transportMessage)
        {
            return Esb.ProcessingStatus.Active;
        }

        public void ProcessingCompleted(TransportMessage transportMessage)
        {
        }

        public bool AddDeferredMessage(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage,
            Stream deferredTransportMessageStream)
        {
            return false;
        }

        public IEnumerable<Stream> GetDeferredMessages(TransportMessage transportMessage)
        {
            return _empty;
        }

        public void DeferredMessageSent(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage)
        {
        }

        public void MessageHandled(TransportMessage transportMessage)
        {
        }
    }
}