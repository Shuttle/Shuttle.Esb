using System.IO;

namespace Shuttle.Esb
{
	public class ReceivedMessage
	{
		public ReceivedMessage(Stream stream, object acknowledgementToken)
		{
			Stream = stream;
			AcknowledgementToken = acknowledgementToken;
		}

		public Stream Stream { get; private set; }
		public object AcknowledgementToken { get; private set; }
	}
}