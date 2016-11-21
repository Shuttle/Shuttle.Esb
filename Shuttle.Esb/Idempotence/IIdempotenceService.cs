using System.Collections.Generic;
using System.IO;

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
		void ProcessingCompleted(TransportMessage transportMessage);

        bool AddDeferredMessage(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage,
			Stream deferredTransportMessageStream);

		IEnumerable<Stream> GetDeferredMessages(TransportMessage transportMessage);
		void DeferredMessageSent(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage);
		void MessageHandled(TransportMessage transportMessage);
	}
}