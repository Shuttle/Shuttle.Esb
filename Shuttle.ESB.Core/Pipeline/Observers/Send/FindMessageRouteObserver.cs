using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
	public class FindMessageRouteObserver : IPipelineObserver<OnFindRouteForMessage>
	{
		public void Execute(OnFindRouteForMessage pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var transportMessage = state.GetTransportMessage();

			if (string.IsNullOrEmpty(transportMessage.RecipientInboxWorkQueueUri))
			{
				transportMessage.RecipientInboxWorkQueueUri = FindRoute(state.GetServiceBus().Configuration.MessageRouteProvider, transportMessage.MessageType);
			}
		}

		private static string FindRoute(IMessageRouteProvider routeProvider, string messageType)
		{
			if (routeProvider == null)
			{
				throw new ESBConfigurationException(ESBResources.NoMessageRouteProviderException);
			}

			var routeUris = routeProvider.GetRouteUris(messageType).ToList();

			if (!routeUris.Any())
			{
				throw new SendMessageException(string.Format(ESBResources.MessageRouteNotFound, messageType));
			}

			if (routeUris.Count() > 1)
			{
				throw new SendMessageException(string.Format(ESBResources.MessageRoutedToMoreThanOneEndpoint,
				                                             messageType, string.Join(",", routeUris.ToArray())));
			}

			return routeUris.ElementAt(0);
		}
	}
}