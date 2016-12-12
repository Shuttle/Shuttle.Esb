using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class QueueManagerConfigurator : IConfigurator
    {
        public void Apply(IServiceBusConfiguration configuration)
        {
            if (ServiceBusConfiguration.ServiceBusSection != null &&
                ServiceBusConfiguration.ServiceBusSection.QueueFactories != null)
            {
                foreach (QueueFactoryElement queueFactoryElement in ServiceBusConfiguration.ServiceBusSection.QueueFactories)
                {
                    var type = Type.GetType(queueFactoryElement.Type);

                    Guard.Against<EsbConfigurationException>(type == null,
                        string.Format(EsbResources.UnknownTypeException, queueFactoryElement.Type));

                    configuration.AddQueueFactoryType(type);
                }

                configuration.ScanForQueueFactories = ServiceBusConfiguration.ServiceBusSection.QueueFactories.Scan;
            }
        }
    }
}