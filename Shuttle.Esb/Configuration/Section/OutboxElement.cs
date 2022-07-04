using System;
using System.Configuration;

namespace Shuttle.Esb
{
    public class OutboxElement : ConfigurationElement
    {
        [ConfigurationProperty("workQueueUri", IsRequired = true)]
        public string WorkQueueUri => (string) this["workQueueUri"];

        [ConfigurationProperty("errorQueueUri", IsRequired = true)]
        public string ErrorQueueUri => (string) this["errorQueueUri"];

        [ConfigurationProperty("durationToSleepWhenIdle", IsRequired = false)]
        public TimeSpan[] DurationToSleepWhenIdle => (TimeSpan[]) this["durationToSleepWhenIdle"];

        [ConfigurationProperty("durationToIgnoreOnFailure", IsRequired = false)]
        public TimeSpan[] DurationToIgnoreOnFailure => (TimeSpan[]) this["durationToIgnoreOnFailure"];

        [ConfigurationProperty("maximumFailureCount", IsRequired = false)]
        public int MaximumFailureCount => (int) this["maximumFailureCount"];

        [ConfigurationProperty("threadCount", IsRequired = false)]
        public int ThreadCount => (int) this["threadCount"];
    }
}