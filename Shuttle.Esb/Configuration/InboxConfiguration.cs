using System;

namespace Shuttle.Esb
{
    public class InboxConfiguration : IInboxConfiguration
    {
        private int _threadCount;

        public InboxConfiguration()
        {
            ThreadCount = 5;
            MaximumFailureCount = 5;
            DistributeSendCount = 5;

            DurationToSleepWhenIdle = new[]
            {
                TimeSpan.FromMilliseconds(250),
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5)
            };

            DurationToIgnoreOnFailure = new[]
            {
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMinutes(30),
                TimeSpan.FromHours(1)
            };
        }

        public IBrokerEndpoint BrokerEndpoint { get; set; }
        public string Uri { get; set; }
        public IBrokerEndpoint ErrorBrokerEndpoint { get; set; }
        public string ErrorUri { get; set; }
        public bool Distribute { get; set; }
        public int DistributeSendCount { get; set; }
        public IBrokerEndpoint DeferredBrokerEndpoint { get; set; }
        public string DeferredUri { get; set; }
        public DeferredMessageProcessor DeferredMessageProcessor { get; set; }

        public int ThreadCount
        {
            get { return _threadCount; }
            set
            {
                _threadCount = value > 0
                    ? value
                    : 5;
            }
        }

        public int MaximumFailureCount { get; set; }
        public TimeSpan[] DurationToIgnoreOnFailure { get; set; }
        public TimeSpan[] DurationToSleepWhenIdle { get; set; }

        public bool HasDeferredBrokerEndpoint => DeferredBrokerEndpoint != null;
    }
}