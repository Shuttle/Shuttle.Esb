using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class OutboxConfigurator : IConfigurator
    {
        public void Apply(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            if (ServiceBusSection.Get() == null
                ||
                ServiceBusSection.Get().Outbox == null
                ||
                string.IsNullOrEmpty(ServiceBusSection.Get().Outbox.WorkQueueUri))
            {
                return;
            }

            //configuration.Outbox =
            //    new OutboxQueueConfiguration
            //    {
            //        WorkQueueUri = ServiceBusSection.Get().Outbox.WorkQueueUri,
            //        ErrorQueueUri = ServiceBusSection.Get().Outbox.ErrorQueueUri,
            //        MaximumFailureCount = ServiceBusSection.Get().Outbox.MaximumFailureCount,
            //        DurationToIgnoreOnFailure =
            //            ServiceBusSection.Get().Outbox.DurationToIgnoreOnFailure ??
            //            ServiceBusConfiguration.DefaultDurationToIgnoreOnFailure,
            //        DurationToSleepWhenIdle =
            //            ServiceBusSection.Get().Outbox.DurationToSleepWhenIdle ??
            //            ServiceBusConfiguration.DefaultDurationToSleepWhenIdle,
            //        ThreadCount = ServiceBusSection.Get().Outbox.ThreadCount
            //    };
        }
    }
}