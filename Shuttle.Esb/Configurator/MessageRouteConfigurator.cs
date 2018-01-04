using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class MessageRouteConfigurator : IConfigurator
    {
        public void Apply(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            if (ServiceBusSection.Get() == null ||
                ServiceBusSection.Get().MessageRoutes == null)
            {
                return;
            }

            foreach (MessageRouteElement mapElement in ServiceBusSection.Get().MessageRoutes)
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