using System.Collections.Generic;
using System.IO;

namespace Shuttle.ESB.Core
{
	public enum ProcessingStatus
	{
		Ignore = 0,
		MessageHandled = 1,
		Assigned = 2
	}

	public interface IIdempotenceService
	{
		ProcessingStatus ProcessingStatus(TransportMessage transportMessage);
		void ProcessingCompleted(TransportMessage transportMessage);
		void AddDeferredMessage(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage, Stream deferredTransportMessageStream);
		IEnumerable<Stream> GetDeferredMessages(TransportMessage transportMessage);
		void DeferredMessageSent(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage);
		void MessageHandled(TransportMessage transportMessage);
	}
}