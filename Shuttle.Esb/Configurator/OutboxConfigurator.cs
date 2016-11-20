using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class OutboxConfigurator : IConfigurator
	{
		public void Apply(IServiceBusConfiguration configuration)
		{
			Guard.AgainstNull(configuration, "configuration");

			if (ServiceBusConfiguration.ServiceBusSection == null
			    ||
			    ServiceBusConfiguration.ServiceBusSection.Outbox == null
			    ||
			    string.IsNullOrEmpty(ServiceBusConfiguration.ServiceBusSection.Outbox.WorkQueueUri))
			{
				return;
			}

			configuration.Outbox =
				new OutboxQueueConfiguration
				{
					WorkQueueUri = ServiceBusConfiguration.ServiceBusSection.Outbox.WorkQueueUri,
					ErrorQueueUri = ServiceBusConfiguration.ServiceBusSection.Outbox.ErrorQueueUri,
					MaximumFailureCount = ServiceBusConfiguration.ServiceBusSection.Outbox.MaximumFailureCount,
					DurationToIgnoreOnFailure =
						ServiceBusConfiguration.ServiceBusSection.Outbox.DurationToIgnoreOnFailure ??
						ServiceBusConfiguration.DefaultDurationToIgnoreOnFailure,
					DurationToSleepWhenIdle =
						ServiceBusConfiguration.ServiceBusSection.Outbox.DurationToSleepWhenIdle ??
						ServiceBusConfiguration.DefaultDurationToSleepWhenIdle,
					ThreadCount = ServiceBusConfiguration.ServiceBusSection.Inbox.ThreadCount
				};
		}
	}
}