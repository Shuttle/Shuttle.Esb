using System;

namespace Shuttle.Esb
{
    public class ControlInboxSettings : ProcessorSettings
    {
        public TimeSpan[] DurationToSleepWhenIdle { get; set; }
        public TimeSpan[] DurationToIgnoreOnFailure { get; set; }
    }
}