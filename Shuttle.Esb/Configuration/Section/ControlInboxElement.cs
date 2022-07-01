using System;
using System.ComponentModel;
using System.Configuration;
using Shuttle.Core.TimeSpanTypeConverters;

namespace Shuttle.Esb
{
    public class ControlInboxElement : ConfigurationElement
    {
        [ConfigurationProperty("uri", IsRequired = true, DefaultValue = "")]
        public string Uri => (string) this["uri"];

        [ConfigurationProperty("errorUri", IsRequired = true, DefaultValue = "")]
        public string ErrorUri => (string) this["errorUri"];

        [ConfigurationProperty("threadCount", IsRequired = false, DefaultValue = 1)]
        public int ThreadCount => (int) this["threadCount"];

        [TypeConverter(typeof(StringDurationArrayConverter))]
        [ConfigurationProperty("durationToSleepWhenIdle", IsRequired = false, DefaultValue = null)]
        public TimeSpan[] DurationToSleepWhenIdle => (TimeSpan[]) this["durationToSleepWhenIdle"];

        [TypeConverter(typeof(StringDurationArrayConverter))]
        [ConfigurationProperty("durationToIgnoreOnFailure", IsRequired = false, DefaultValue = null)]
        public TimeSpan[] DurationToIgnoreOnFailure => (TimeSpan[]) this["durationToIgnoreOnFailure"];

        [ConfigurationProperty("maximumFailureCount", IsRequired = false, DefaultValue = 5)]
        public int MaximumFailureCount => (int) this["maximumFailureCount"];
    }
}