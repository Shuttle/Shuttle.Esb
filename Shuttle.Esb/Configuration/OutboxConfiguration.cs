using System;

namespace Shuttle.Esb
{
    public class OutboxConfiguration : IOutboxConfiguration
    {
        private int threadCount;

        public OutboxConfiguration()
        {
            ThreadCount = 1;
            MaximumFailureCount = 5;

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
        public IBrokerEndpoint ErrorBrokerEndpoint { get; set; }

        public int ThreadCount
        {
            get { return threadCount; }
            set
            {
                threadCount = value > 0
                    ? value
                    : 5;
            }
        }

        public int MaximumFailureCount { get; set; }
        public TimeSpan[] DurationToIgnoreOnFailure { get; set; }
        public TimeSpan[] DurationToSleepWhenIdle { get; set; }
        public string Uri { get; set; }
        public string ErrorUri { get; set; }
    }
}