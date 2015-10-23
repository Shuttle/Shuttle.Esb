namespace Shuttle.ESB.Core
{
	public class MessageRouteProviderConfigurator : IConfigurator
	{
		public void Apply(IServiceBusConfiguration configuration)
		{
			if (ServiceBusConfiguration.ServiceBusSection == null ||
			    ServiceBusConfiguration.ServiceBusSection.MessageRoutes == null)
			{
				return;
			}

			var specificationFactory = new MessageRouteSpecificationFactory();
			var provider = configuration.MessageRouteProvider;

			foreach (MessageRouteElement mapElement in ServiceBusConfiguration.ServiceBusSection.MessageRoutes)
			{
				var messageRoute = provider.Find(mapElement.Uri);

				if (messageRoute == null)
				{
					messageRoute = new MessageRoute(configuration.QueueManager.GetQueue(mapElement.Uri));

					provider.Add(messageRoute);
				}

				foreach (SpecificationElement specificationElement in mapElement)
				{
					messageRoute.AddSpecification(specificationFactory.Create(specificationElement.Name, specificationElement.Value));
				}
			}
		}
	}
}