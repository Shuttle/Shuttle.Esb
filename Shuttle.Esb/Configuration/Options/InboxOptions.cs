using System;

namespace Shuttle.Esb
{
    public class InboxOptions : ProcessorOptions
    {
        public InboxOptions()
        {
            ThreadCount = 5;
        }

        public string DeferredQueueUri { get; set; }
        public bool Distribute { get; set; } = false;
        public int DistributeSendCount { get; set; } = 5;
        public TimeSpan DeferredMessageProcessorWaitInterval { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan DeferredMessageProcessorResetInterval { get; set; } = TimeSpan.FromMinutes(1);
    }
}