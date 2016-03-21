using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class QueueManagerConfigurator : IConfigurator
	{
		public void Apply(IServiceBusConfiguration configuration)
		{
			if (ServiceBusConfiguration.ServiceBusSection == null ||
			    ServiceBusConfiguration.ServiceBusSection.QueueFactories == null)
			{
				return;
			}

			foreach (QueueFactoryElement queueFactoryElement in ServiceBusConfiguration.ServiceBusSection.QueueFactories)
			{
				var type = Type.GetType(queueFactoryElement.Type);

				Guard.Against<ESBConfigurationException>(type == null,
					string.Format(ESBResources.UnknownTypeException, queueFactoryElement.Type));

				configuration.QueueManager.RegisterQueueFactory(type);
			}

			if (ServiceBusConfiguration.ServiceBusSection.QueueFactories.Scan)
			{
				configuration.QueueManager.ScanForQueueFactories();
			}
		}
	}
}