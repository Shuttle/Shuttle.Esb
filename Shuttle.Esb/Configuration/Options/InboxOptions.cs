using System;

namespace Shuttle.Esb;

public class InboxOptions : ProcessorOptions
{
    public InboxOptions()
    {
        ThreadCount = 5;
    }

    public TimeSpan DeferredMessageProcessorResetInterval { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan DeferredMessageProcessorWaitInterval { get; set; } = TimeSpan.FromSeconds(1);

    public string DeferredQueueUri { get; set; } = string.Empty;
}