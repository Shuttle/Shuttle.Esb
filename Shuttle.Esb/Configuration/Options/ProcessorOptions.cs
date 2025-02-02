using System;
using System.Collections.Generic;

namespace Shuttle.Esb;

public class ProcessorOptions
{
    public List<TimeSpan> DurationToIgnoreOnFailure { get; set; } = new();
    public List<TimeSpan> DurationToSleepWhenIdle { get; set; } = new();
    public string? ErrorQueueUri { get; set; }
    public int MaximumFailureCount { get; set; } = 5;
    public int ThreadCount { get; set; } = 1;
    public string? WorkQueueUri { get; set; }
}