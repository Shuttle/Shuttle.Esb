using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public interface IServiceBusConfiguration
    {
        IControlInboxConfiguration ControlInbox { get; }
        IInboxConfiguration Inbox { get; }
        IOutboxConfiguration Outbox { get; }
        IWorkerConfiguration Worker { get; }

        IEnumerable<MessageRouteConfiguration> MessageRoutes { get; }
        IEnumerable<UriMappingConfiguration> UriMapping { get; }
        void AddMessageRoute(MessageRouteConfiguration messageRoute);
        void AddUriMapping(Uri sourceUri, Uri targetUri);
    }
}