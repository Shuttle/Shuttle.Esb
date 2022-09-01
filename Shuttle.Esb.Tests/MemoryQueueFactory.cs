using System;

namespace Shuttle.Esb.Tests
{
    public class MemoryQueueFactory : IQueueFactory
    {
        public string Scheme => "memory";
        public IQueue Create(Uri uri)
        {
            return new MemoryQueue(uri);
        }
    }
}