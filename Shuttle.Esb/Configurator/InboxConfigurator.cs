using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class InboxConfigurator : IConfigurator
	{
		public void Apply(IServiceBusConfiguration configuration)
		{
			Guard.AgainstNull(configuration, "configuration");

			if (ServiceBusConfiguration.ServiceBusSection == null
			    ||
			    ServiceBusConfiguration.ServiceBusSection.Inbox == null
			    ||
			    string.IsNullOrEmpty(ServiceBusConfiguration.ServiceBusSection.Inbox.WorkQueueUri))
			{
				return;
			}

			configuration.Inbox =
				new InboxQueueConfiguration
				{
					WorkQueueUri = ServiceBusConfiguration.ServiceBusSection.Inbox.WorkQueueUri,
                    DeferredQueueUri =ServiceBusConfiguration.ServiceBusSection.Inbox.DeferredQueueUri,
                    ErrorQueueUri = ServiceBusConfiguration.ServiceBusSection.Inbox.ErrorQueueUri,
					ThreadCount = ServiceBusConfiguration.ServiceBusSection.Inbox.ThreadCount,
					MaximumFailureCount = ServiceBusConfiguration.ServiceBusSection.Inbox.MaximumFailureCount,
					DurationToIgnoreOnFailure =
						ServiceBusConfiguration.ServiceBusSection.Inbox.DurationToIgnoreOnFailure ??
						ServiceBusConfiguration.DefaultDurationToIgnoreOnFailure,
					DurationToSleepWhenIdle =
						ServiceBusConfiguration.ServiceBusSection.Inbox.DurationToSleepWhenIdle ??
						ServiceBusConfiguration.DefaultDurationToSleepWhenIdle,
					Distribute = ServiceBusConfiguration.ServiceBusSection.Inbox.Distribute,
					DistributeSendCount = ServiceBusConfiguration.ServiceBusSection.Inbox.DistributeSendCount
				};
		}
	}
}