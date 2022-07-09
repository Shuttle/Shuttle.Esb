using System;

namespace Shuttle.Esb
{
    public class InboxQueueConfiguration : IInboxQueueConfiguration
    {
        public IQueue WorkQueue { get; set; }
        public IQueue ErrorQueue { get; set; }
        public IQueue DeferredQueue { get; set; }
        public DeferredMessageProcessor DeferredMessageProcessor { get; set; }
    }
}