using System;
using System.ComponentModel;
using System.Configuration;
using Shuttle.Core.TimeSpanTypeConverters;

namespace Shuttle.Esb
{
    public class InboxElement : ConfigurationElement
    {
        [ConfigurationProperty("workQueueUri", IsRequired = true)]
        public string WorkQueueUri => (string) this["workQueueUri"];

        [ConfigurationProperty("deferredQueueUri", IsRequired = false, DefaultValue = "")]
        public string DeferredQueueUri => (string) this["deferredQueueUri"];

        [ConfigurationProperty("errorQueueUri", IsRequired = true)]
        public string ErrorQueueUri => (string) this["errorQueueUri"];

        [ConfigurationProperty("threadCount", IsRequired = false, DefaultValue = 5)]
        public int ThreadCount => (int) this["threadCount"];

        [TypeConverter(typeof(StringDurationArrayConverter))]
        [ConfigurationProperty("durationToSleepWhenIdle", IsRequired = false, DefaultValue = null)]
        public TimeSpan[] DurationToSleepWhenIdle => (TimeSpan[]) this["durationToSleepWhenIdle"];

        [TypeConverter(typeof(StringDurationArrayConverter))]
        [ConfigurationProperty("durationToIgnoreOnFailure", IsRequired = false, DefaultValue = null)]
        public TimeSpan[] DurationToIgnoreOnFailure => (TimeSpan[]) this["durationToIgnoreOnFailure"];

        [ConfigurationProperty("maximumFailureCount", IsRequired = false, DefaultValue = 5)]
        public int MaximumFailureCount => (int) this["maximumFailureCount"];

        [ConfigurationProperty("distribute", IsRequired = false, DefaultValue = false)]
        public bool Distribute => (bool) this["distribute"];

        [ConfigurationProperty("distributeSendCount", IsRequired = false, DefaultValue = 3)]
        public int DistributeSendCount => (int) this["distributeSendCount"];
    }
}