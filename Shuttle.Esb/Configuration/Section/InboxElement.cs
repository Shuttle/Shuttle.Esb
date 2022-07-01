using System;
using System.ComponentModel;
using System.Configuration;
using Shuttle.Core.TimeSpanTypeConverters;

namespace Shuttle.Esb
{
    public class InboxElement : ConfigurationElement
    {
        [ConfigurationProperty("uri", IsRequired = true)]
        public string Uri => (string) this["uri"];

        [ConfigurationProperty("deferredUri", IsRequired = false, DefaultValue = "")]
        public string DeferredUri => (string) this["deferredUri"];

        [ConfigurationProperty("errorUri", IsRequired = true)]
        public string ErrorUri => (string) this["errorUri"];

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