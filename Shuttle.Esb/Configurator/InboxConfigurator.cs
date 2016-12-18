using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class InboxConfigurator : IConfigurator
	{
		public void Apply(IServiceBusConfiguration configuration)
		{
			Guard.AgainstNull(configuration, "configuration");

			if (ServiceBusSection.Get() == null
			    ||
			    ServiceBusSection.Get().Inbox == null
			    ||
			    string.IsNullOrEmpty(ServiceBusSection.Get().Inbox.WorkQueueUri))
			{
				return;
			}

			configuration.Inbox =
				new InboxQueueConfiguration
				{
					WorkQueueUri = ServiceBusSection.Get().Inbox.WorkQueueUri,
                    DeferredQueueUri =ServiceBusSection.Get().Inbox.DeferredQueueUri,
                    ErrorQueueUri = ServiceBusSection.Get().Inbox.ErrorQueueUri,
					ThreadCount = ServiceBusSection.Get().Inbox.ThreadCount,
					MaximumFailureCount = ServiceBusSection.Get().Inbox.MaximumFailureCount,
					DurationToIgnoreOnFailure =
						ServiceBusSection.Get().Inbox.DurationToIgnoreOnFailure ??
						ServiceBusConfiguration.DefaultDurationToIgnoreOnFailure,
					DurationToSleepWhenIdle =
						ServiceBusSection.Get().Inbox.DurationToSleepWhenIdle ??
						ServiceBusConfiguration.DefaultDurationToSleepWhenIdle,
					Distribute = ServiceBusSection.Get().Inbox.Distribute,
					DistributeSendCount = ServiceBusSection.Get().Inbox.DistributeSendCount
				};
		}
	}
}