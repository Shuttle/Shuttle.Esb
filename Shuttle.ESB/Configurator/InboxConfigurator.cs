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
					WorkQueue = configuration.QueueManager.GetQueue(ServiceBusConfiguration.ServiceBusSection.Inbox.WorkQueueUri),
					ErrorQueue = configuration.QueueManager.GetQueue(ServiceBusConfiguration.ServiceBusSection.Inbox.ErrorQueueUri),
					ThreadCount = ServiceBusConfiguration.ServiceBusSection.Inbox.ThreadCount,
					MaximumFailureCount = ServiceBusConfiguration.ServiceBusSection.Inbox.MaximumFailureCount,
					DurationToIgnoreOnFailure =
						ServiceBusConfiguration.ServiceBusSection.Inbox.DurationToIgnoreOnFailure ?? ServiceBusConfiguration.DefaultDurationToIgnoreOnFailure,
					DurationToSleepWhenIdle =
						ServiceBusConfiguration.ServiceBusSection.Inbox.DurationToSleepWhenIdle ?? ServiceBusConfiguration.DefaultDurationToSleepWhenIdle,
					Distribute = ServiceBusConfiguration.ServiceBusSection.Inbox.Distribute,
					DistributeSendCount = ServiceBusConfiguration.ServiceBusSection.Inbox.DistributeSendCount,
					DeferredQueue =
						string.IsNullOrEmpty(ServiceBusConfiguration.ServiceBusSection.Inbox.DeferredQueueUri)
							? null
							: configuration.QueueManager.GetQueue(ServiceBusConfiguration.ServiceBusSection.Inbox.DeferredQueueUri)
				};
		}
	}
}