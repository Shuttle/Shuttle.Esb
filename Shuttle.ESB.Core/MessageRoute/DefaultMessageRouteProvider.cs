using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
	public sealed class DefaultMessageRouteProvider : IMessageRouteProvider, IRequireInitialization
	{
		private readonly IMessageRouteCollection _messageRoutes = new MessageRouteCollection();

		public IEnumerable<string> GetRouteUris(string messageType)
		{
			var uri = _messageRoutes.FindAll(messageType).Select(messageRoute => messageRoute.Queue.Uri.ToString()).FirstOrDefault();

			return
				string.IsNullOrEmpty(uri)
					? new List<string>()
					: new List<string> {uri};
		}

		public void AddMessageRoute(IMessageRoute messageRoute)
		{
			Guard.AgainstNull(messageRoute, "messageRoute");

			var existing = _messageRoutes.Find(messageRoute.Queue.Uri);

			if (existing == null)
			{
				_messageRoutes.Add(messageRoute);
			}
			else
			{
				foreach (var specification in messageRoute.Specifications)
				{
					existing.AddSpecification(specification);
				}
			}
		}

		public void Initialize(IServiceBus bus)
		{
			if (ServiceBusConfiguration.ServiceBusSection == null || ServiceBusConfiguration.ServiceBusSection.MessageRoutes == null)
			{
				return;
			}

			var factory = new MessageRouteSpecificationFactory();

			foreach (MessageRouteElement mapElement in ServiceBusConfiguration.ServiceBusSection.MessageRoutes)
			{
				var messageRoute = _messageRoutes.Find(mapElement.Uri);

				if (messageRoute == null)
				{
					messageRoute = new MessageRoute(bus.Configuration.QueueManager.GetQueue(mapElement.Uri));

					_messageRoutes.Add(messageRoute);
				}

				foreach (SpecificationElement specificationElement in mapElement)
				{
					messageRoute.AddSpecification(factory.Create(specificationElement.Name, specificationElement.Value));
				}
			}
		}
	}
}