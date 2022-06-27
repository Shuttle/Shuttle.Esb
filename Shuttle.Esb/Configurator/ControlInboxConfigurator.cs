using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ControlInboxConfigurator : IConfigurator
    {
        public void Apply(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            if (ServiceBusSection.Get() == null)
            {
                return;
            }

            var controlInboxElement = ServiceBusSection.Get().ControlInbox;

            if (controlInboxElement == null
                ||
                string.IsNullOrEmpty(controlInboxElement.WorkQueueUri)
                ||
                string.IsNullOrEmpty(controlInboxElement.ErrorQueueUri))
            {
                return;
            }

            //configuration.ControlInbox =
            //    new ControlInboxQueueConfiguration
            //    {
            //        WorkQueueUri = controlInboxElement.WorkQueueUri,
            //        ErrorQueueUri = controlInboxElement.ErrorQueueUri,
            //        ThreadCount = controlInboxElement.ThreadCount,
            //        MaximumFailureCount = controlInboxElement.MaximumFailureCount,
            //        DurationToIgnoreOnFailure =
            //            controlInboxElement.DurationToIgnoreOnFailure ??
            //            ServiceBusConfiguration.DefaultDurationToIgnoreOnFailure,
            //        DurationToSleepWhenIdle =
            //            controlInboxElement.DurationToSleepWhenIdle ??
            //            ServiceBusConfiguration.DefaultDurationToSleepWhenIdle
            //    };
        }
    }
}