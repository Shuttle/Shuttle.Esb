using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class ControlInboxConfigurator : IConfigurator
	{
		public void Apply(IServiceBusConfiguration configuration)
		{
			Guard.AgainstNull(configuration, "configuration");

			if (ServiceBusConfiguration.ServiceBusSection == null)
			{
				return;
			}

			var controlInboxElement = ServiceBusConfiguration.ServiceBusSection.ControlInbox;

			if (controlInboxElement == null
			    ||
			    string.IsNullOrEmpty(controlInboxElement.WorkQueueUri)
			    ||
			    string.IsNullOrEmpty(controlInboxElement.ErrorQueueUri))
			{
				return;
			}

			configuration.ControlInbox =
				new ControlInboxQueueConfiguration
				{
					WorkQueue = configuration.QueueManager.GetQueue(controlInboxElement.WorkQueueUri),
					ErrorQueue = configuration.QueueManager.GetQueue(controlInboxElement.ErrorQueueUri),
					ThreadCount = controlInboxElement.ThreadCount,
					MaximumFailureCount = controlInboxElement.MaximumFailureCount,
					DurationToIgnoreOnFailure =
						controlInboxElement.DurationToIgnoreOnFailure ?? ServiceBusConfiguration.DefaultDurationToIgnoreOnFailure,
					DurationToSleepWhenIdle =
						controlInboxElement.DurationToSleepWhenIdle ?? ServiceBusConfiguration.DefaultDurationToSleepWhenIdle
				};
		}
	}
}