using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IFindMessageRouteObserver : IPipelineObserver<OnFindRouteForMessage>
    {
    }

    public class FindMessageRouteObserver : IFindMessageRouteObserver
    {
        private readonly IMessageRouteProvider _messageRouteProvider;

        public FindMessageRouteObserver(IMessageRouteProvider messageRouteProvider)
        {
            Guard.AgainstNull(messageRouteProvider, nameof(messageRouteProvider));

            _messageRouteProvider = messageRouteProvider;
        }

        public void Execute(OnFindRouteForMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();

            if (string.IsNullOrEmpty(transportMessage.RecipientInboxWorkQueueUri))
            {
                transportMessage.RecipientInboxWorkQueueUri =
                    FindRoute(_messageRouteProvider, transportMessage.MessageType);
            }
        }

        private static string FindRoute(IMessageRouteProvider routeProvider, string messageType)
        {
            var routeUris = routeProvider.GetRouteUris(messageType).ToList();

            if (!routeUris.Any())
            {
                throw new SendMessageException(string.Format(Resources.MessageRouteNotFound, messageType));
            }

            if (routeUris.Count() > 1)
            {
                throw new SendMessageException(string.Format(Resources.MessageRoutedToMoreThanOneEndpoint,
                    messageType, string.Join(",", routeUris.ToArray())));
            }

            return routeUris.ElementAt(0);
        }
    }
}