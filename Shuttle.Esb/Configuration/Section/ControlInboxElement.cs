using System;
using System.Configuration;

namespace Shuttle.Esb
{
    public class ControlInboxElement : ConfigurationElement
    {
        [ConfigurationProperty("workQueueUri", IsRequired = true, DefaultValue = "")]
        public string WorkQueueUri => (string) this["workQueueUri"];

        [ConfigurationProperty("errorQueueUri", IsRequired = true, DefaultValue = "")]
        public string ErrorQueueUri => (string) this["errorQueueUri"];

        [ConfigurationProperty("threadCount", IsRequired = false, DefaultValue = 1)]
        public int ThreadCount => (int) this["threadCount"];

        [ConfigurationProperty("durationToSleepWhenIdle", IsRequired = false, DefaultValue = null)]
        public TimeSpan[] DurationToSleepWhenIdle => (TimeSpan[]) this["durationToSleepWhenIdle"];

        [ConfigurationProperty("durationToIgnoreOnFailure", IsRequired = false, DefaultValue = null)]
        public TimeSpan[] DurationToIgnoreOnFailure => (TimeSpan[]) this["durationToIgnoreOnFailure"];

        [ConfigurationProperty("maximumFailureCount", IsRequired = false, DefaultValue = 5)]
        public int MaximumFailureCount => (int) this["maximumFailureCount"];
    }
}