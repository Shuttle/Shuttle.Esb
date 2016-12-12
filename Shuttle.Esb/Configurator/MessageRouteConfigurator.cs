using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class MessageRouteConfigurator : IConfigurator
    {
        public void Apply(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, "configuration");

            if (ServiceBusConfiguration.ServiceBusSection == null ||
                ServiceBusConfiguration.ServiceBusSection.MessageRoutes == null)
            {
                return;
            }

            foreach (MessageRouteElement mapElement in ServiceBusConfiguration.ServiceBusSection.MessageRoutes)
            {
                var messageRoute = new MessageRouteConfiguration(mapElement.Uri);

                foreach (SpecificationElement specificationElement in mapElement)
                {
                    messageRoute.AddSpecification(specificationElement.Name, specificationElement.Value);
                }

                if (messageRoute.Specifications.Any())
                {
                    configuration.AddMessageRoute(messageRoute);
                }
            }
        }
    }
}