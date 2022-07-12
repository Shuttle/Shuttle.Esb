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
        private readonly List<Type> _modules = new List<Type>();

        public IInboxConfiguration Inbox { get; set; }
        public IControlInboxConfiguration ControlInbox { get; set; }
        public IOutboxConfiguration Outbox { get; set; }
        public IWorkerConfiguration Worker { get; set; }

        public void AddMessageRoute(MessageRouteConfiguration messageRoute)
        {
            Guard.AgainstNull(messageRoute, nameof(messageRoute));

            _messageRoutes.Add(messageRoute);
        }

        public IEnumerable<UriMappingConfiguration> UriMapping => _uriMapping.AsReadOnly();

        public void AddUriMapping(Uri sourceUri, Uri targetUri)
        {
            _uriMapping.Add(new UriMappingConfiguration(sourceUri, targetUri));
        }

        public IEnumerable<Type> Modules => _modules.AsReadOnly();

        public void AddModule(Type type)
        {
            Guard.AgainstNull(type, nameof(type));

            _modules.Add(type);
        }
    }
}