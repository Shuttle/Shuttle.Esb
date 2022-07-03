using System;
using System.IO;

namespace Shuttle.Esb.Tests
{
    public class NullQueue : IQueue
    {
        public NullQueue(string uri)
            : this(new Uri(uri))
        {
        }

        public NullQueue(Uri uri)
        {
            Uri = uri;
        }

        public Uri Uri { get; }
        public bool IsStream => false;

        public bool IsEmpty()
        {
            return true;
        }

        public void Enqueue(TransportMessage transportMessage, Stream stream)
        {
            throw new NotImplementedException();
        }

        public ReceivedMessage GetMessage()
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