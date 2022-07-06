using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class InboxSettings : ProcessorSettings
    {
        public string DeferredQueueUri { get; set; }
        public IQueue DeferredQueue { get; set; }
        public TimeSpan[] DurationToSleepWhenIdle { get; set; }
        public TimeSpan[] DurationToIgnoreOnFailure { get; set; }
        public bool Distribute { get; set; } = false;
        public int DistributeSendCount { get; set; } = 3;

        public void Apply(InboxSettings settings)
        {
            Guard.AgainstNull(settings, nameof(settings));

            base.Apply(settings);

            DeferredQueueUri = settings.DeferredQueueUri;
            Distribute = settings.Distribute;
            DistributeSendCount = settings.DistributeSendCount;
            DurationToIgnoreOnFailure = settings.DurationToIgnoreOnFailure;
            DurationToSleepWhenIdle = settings.DurationToSleepWhenIdle;
        }
    }
}