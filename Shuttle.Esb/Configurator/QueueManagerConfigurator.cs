using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class QueueManagerConfigurator : IConfigurator
    {
        public void Apply(IServiceBusConfiguration configuration)
        {
            if (ServiceBusSection.Get() != null &&
                ServiceBusSection.Get().QueueFactories != null)
            {
                var reflectionService = new ReflectionService();

                foreach (QueueFactoryElement queueFactoryElement in ServiceBusSection.Get().QueueFactories)
                {
                    var type = reflectionService.GetType(queueFactoryElement.Type);

                    Guard.Against<EsbConfigurationException>(type == null,
                        string.Format(Resources.UnknownTypeException, queueFactoryElement.Type));

                    configuration.AddQueueFactoryType(type);
                }

                configuration.ScanForQueueFactories = ServiceBusSection.Get().QueueFactories.Scan;
            }
        }
    }
}