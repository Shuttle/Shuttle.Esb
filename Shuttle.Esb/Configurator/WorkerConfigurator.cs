using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class WorkerConfigurator : IConfigurator
	{
		public void Apply(IServiceBusConfiguration configuration)
		{
			Guard.AgainstNull(configuration, "configuration");

			if (ServiceBusSection.Get() == null
			    ||
			    ServiceBusSection.Get().Worker == null
			    ||
			    string.IsNullOrEmpty(ServiceBusSection.Get().Worker.DistributorControlWorkQueueUri))
			{
				return;
			}

		    configuration.Worker = new WorkerConfiguration
		    {
		        DistributorControlInboxWorkQueueUri =
		            ServiceBusSection.Get().Worker.DistributorControlWorkQueueUri,
                ThreadAvailableNotificationIntervalSeconds = ServiceBusSection.Get().Worker.ThreadAvailableNotificationIntervalSeconds
            };
		}
	}
}