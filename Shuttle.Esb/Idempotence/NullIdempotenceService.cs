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

        public void AddDeferredMessage(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage,
            Stream deferredTransportMessageStream)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Stream> GetDeferredMessages(TransportMessage transportMessage)
        {
            throw new System.NotImplementedException();
        }

        public void DeferredMessageSent(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage)
        {
            throw new System.NotImplementedException();
        }

        public void MessageHandled(TransportMessage transportMessage)
        {
        }

        public bool CanDeferMessage {
            get { return false; }
        }
    }
}