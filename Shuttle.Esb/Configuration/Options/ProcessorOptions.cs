using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public class ProcessorOptions
    {
        public string WorkQueueUri { get; set; }
        public string ErrorQueueUri { get; set; }
        public int MaximumFailureCount { get; set; } = 5;
        public int ThreadCount { get; set; } = 1;
        public List<TimeSpan> DurationToSleepWhenIdle { get; set; } = new List<TimeSpan>();
        public List<TimeSpan> DurationToIgnoreOnFailure { get; set; } = new List<TimeSpan>();
    }
}