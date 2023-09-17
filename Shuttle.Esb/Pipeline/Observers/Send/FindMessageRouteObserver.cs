﻿using System.Linq;
using System.Threading.Tasks;
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

            if (!string.IsNullOrEmpty(transportMessage.RecipientInboxWorkQueueUri))
            {
                return;
            }

            var routeUris = _messageRouteProvider.GetRouteUris(transportMessage.MessageType).ToList();

            if (!routeUris.Any())
            {
                throw new SendMessageException(string.Format(Resources.MessageRouteNotFound, transportMessage.MessageType));
            }

            if (routeUris.Count > 1)
            {
                throw new SendMessageException(string.Format(Resources.MessageRoutedToMoreThanOneEndpoint, transportMessage.MessageType, string.Join(",", routeUris.ToArray())));
            }

            transportMessage.RecipientInboxWorkQueueUri = routeUris.ElementAt(0);
        }

        public async Task ExecuteAsync(OnFindRouteForMessage pipelineEvent)
        {
            Execute(pipelineEvent);

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}