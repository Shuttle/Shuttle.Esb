using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ServiceBusConfiguration : IServiceBusConfiguration
    {
        private readonly List<MessageRouteConfiguration> _messageRoutes = new List<MessageRouteConfiguration>();
        private readonly List<UriMappingConfiguration> _uriMapping = new List<UriMappingConfiguration>();

        public ServiceBusConfiguration()
        {
            ShouldCreateQueues = true;
            ShouldCacheIdentity = true;
            ShouldAddMessageHandlers = true;
            ShouldRemoveMessagesNotHandled = false;
        }

        public IInboxQueueConfiguration Inbox { get; set; }
        public IControlInboxQueueConfiguration ControlInbox { get; set; }
        public IOutboxQueueConfiguration Outbox { get; set; }
        public IWorkerConfiguration Worker { get; set; }

        public bool ShouldCreateQueues { get; set; }
        public bool ShouldCacheIdentity { get; set; }
        public bool ShouldAddMessageHandlers { get; set; }

        public bool ShouldRemoveMessagesNotHandled { get; set; }
        public bool ShouldRemoveCorruptMessages { get; set; }
        public string EncryptionAlgorithm { get; set; }
        public string CompressionAlgorithm { get; set; }

        public IEnumerable<MessageRouteConfiguration> MessageRoutes =>
            new ReadOnlyCollection<MessageRouteConfiguration>(_messageRoutes);

        public void AddMessageRoute(MessageRouteConfiguration messageRoute)
        {
            Guard.AgainstNull(messageRoute, nameof(messageRoute));

            _messageRoutes.Add(messageRoute);
        }

        public IEnumerable<UriMappingConfiguration> UriMapping =>
            new ReadOnlyCollection<UriMappingConfiguration>(_uriMapping);

        public void AddUriMapping(Uri sourceUri, Uri targetUri)
        {
            _uriMapping.Add(new UriMappingConfiguration(sourceUri, targetUri));
        }

        public bool IsWorker => Worker != null;
    }
}