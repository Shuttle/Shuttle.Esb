using System;

namespace Shuttle.Esb
{
    public class OutboxSettings : ProcessorSettings
    {
        public TimeSpan[] DurationToSleepWhenIdle { get; set; }
        public TimeSpan[] DurationToIgnoreOnFailure { get; set; }
    }
}