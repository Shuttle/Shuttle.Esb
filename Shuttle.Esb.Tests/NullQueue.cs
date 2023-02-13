using System;
using System.IO;
using System.Threading.Tasks;

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
            Uri = new QueueUri(uri);
        }

        public QueueUri Uri { get; }
        public bool IsStream => false;

        public async ValueTask<bool> IsEmpty()
        {
            return await Task.FromResult(true);
        }

        public Task Enqueue(TransportMessage transportMessage, Stream stream)
        {
            throw new NotImplementedException();
        }

        public Task<ReceivedMessage> GetMessage()
        {
            throw new NotImplementedException();
        }

        public Task Acknowledge(object acknowledgementToken)
        {
            throw new NotImplementedException();
        }

        public Task Release(object acknowledgementToken)
        {
            throw new NotImplementedException();
        }
    }
}