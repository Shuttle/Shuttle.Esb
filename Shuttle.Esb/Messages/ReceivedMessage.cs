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

        public Stream Stream { get; }
        public object AcknowledgementToken { get; }
    }
}