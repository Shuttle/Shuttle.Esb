using System;

namespace Shuttle.Esb
{
    public class OutboxSettings : ProcessorSettings
    {
        public const string SectionName = "Shuttle:ServiceBus:Outbox";

        public TimeSpan[] DurationToSleepWhenIdle { get; set; }
        public TimeSpan[] DurationToIgnoreOnFailure { get; set; }
    }
}