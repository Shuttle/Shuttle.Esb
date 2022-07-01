using System;
using System.ComponentModel;
using System.Configuration;
using Shuttle.Core.TimeSpanTypeConverters;

namespace Shuttle.Esb
{
    public class OutboxElement : ConfigurationElement
    {
        [ConfigurationProperty("uri", IsRequired = true)]
        public string Uri => (string) this["uri"];

        [ConfigurationProperty("errorUri", IsRequired = true)]
        public string ErrorUri => (string) this["errorUri"];

        [TypeConverter(typeof(StringDurationArrayConverter))]
        [ConfigurationProperty("durationToSleepWhenIdle", IsRequired = false)]
        public TimeSpan[] DurationToSleepWhenIdle => (TimeSpan[]) this["durationToSleepWhenIdle"];

        [TypeConverter(typeof(StringDurationArrayConverter))]
        [ConfigurationProperty("durationToIgnoreOnFailure", IsRequired = false)]
        public TimeSpan[] DurationToIgnoreOnFailure => (TimeSpan[]) this["durationToIgnoreOnFailure"];

        [ConfigurationProperty("maximumFailureCount", IsRequired = false)]
        public int MaximumFailureCount => (int) this["maximumFailureCount"];

        [ConfigurationProperty("threadCount", IsRequired = false)]
        public int ThreadCount => (int) this["threadCount"];
    }
}