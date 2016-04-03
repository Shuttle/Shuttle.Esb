using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
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
				throw new EsbConfigurationException(EsbResources.NoMessageRouteProviderException);
			}

			var routeUris = routeProvider.GetRouteUris(messageType).ToList();

			if (!routeUris.Any())
			{
				throw new SendMessageException(string.Format(EsbResources.MessageRouteNotFound, messageType));
			}

			if (routeUris.Count() > 1)
			{
				throw new SendMessageException(string.Format(EsbResources.MessageRoutedToMoreThanOneEndpoint,
				                                             messageType, string.Join(",", routeUris.ToArray())));
			}

			return routeUris.ElementAt(0);
		}
	}
}