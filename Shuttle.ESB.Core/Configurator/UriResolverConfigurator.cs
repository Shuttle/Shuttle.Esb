using System;

namespace Shuttle.ESB.Core
{
	public class UriResolverConfigurator : IConfigurator
	{
		public void Apply(IServiceBusConfiguration configuration)
		{
			if (ServiceBusConfiguration.ServiceBusSection == null ||
				ServiceBusConfiguration.ServiceBusSection.UriResolver == null)
			{
				return;
			}

			foreach (UriResolverItemElement uriRepositoryItemElement in ServiceBusConfiguration.ServiceBusSection.UriResolver)
			{
				configuration.UriResolver.Add(uriRepositoryItemElement.Name.ToLower(), new Uri(uriRepositoryItemElement.Uri));
			}
		}
	}
}