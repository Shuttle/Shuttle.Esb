using System;
using System.IO;

namespace Shuttle.Esb.Tests
{
    public class NullBrokerEndpoint : IBrokerEndpoint
    {
        public NullBrokerEndpoint(string uri)
            : this(new Uri(uri))
        {
        }

        public NullBrokerEndpoint(Uri uri)
        {
            Uri = uri;
        }

        public Uri Uri { get; }

        public bool IsEmpty()
        {
            return true;
        }

        public void Send(TransportMessage transportMessage, Stream stream)
        {
            throw new NotImplementedException();
        }

        public ReceivedMessage Receive()
        {
            throw new NotImplementedException();
        }

        public void Acknowledge(object acknowledgementToken)
        {
            throw new NotImplementedException();
        }

        public void Release(object acknowledgementToken)
        {
            throw new NotImplementedException();
        }
    }
}