using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class QueueManagerConfigurator : IConfigurator
    {
        public void Apply(IServiceBusConfiguration configuration)
        {
            if (ServiceBusSection.Get() != null &&
                ServiceBusSection.Get().QueueFactories != null)
            {
                foreach (QueueFactoryElement queueFactoryElement in ServiceBusSection.Get().QueueFactories)
                {
                    var type = Type.GetType(queueFactoryElement.Type);

                    Guard.Against<EsbConfigurationException>(type == null,
                        string.Format(Resources.UnknownTypeException, queueFactoryElement.Type));

                    configuration.AddQueueFactoryType(type);
                }

                configuration.ScanForQueueFactories = ServiceBusSection.Get().QueueFactories.Scan;
            }
        }
    }
}