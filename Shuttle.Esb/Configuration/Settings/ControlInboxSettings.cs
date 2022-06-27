using System;

namespace Shuttle.Esb
{
    public class ControlInboxSettings : ProcessorSettings
    {
        public const string SectionName = "Shuttle:ServiceBus:ControlInbox";

        public TimeSpan[] DurationToSleepWhenIdle { get; set; }
        public TimeSpan[] DurationToIgnoreOnFailure { get; set; }
    }
}