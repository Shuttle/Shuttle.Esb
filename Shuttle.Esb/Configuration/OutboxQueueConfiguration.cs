using System;

namespace Shuttle.Esb
{
    public class OutboxQueueConfiguration : IOutboxQueueConfiguration
    {
        public IQueue WorkQueue { get; set; }
        public IQueue ErrorQueue { get; set; }
    }
}