using System;

namespace Shuttle.Esb
{
    public class OutboxConfiguration : IOutboxConfiguration
    {
        public IQueue WorkQueue { get; set; }
        public IQueue ErrorQueue { get; set; }
    }
}