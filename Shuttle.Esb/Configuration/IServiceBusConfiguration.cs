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

        IEnumerable<UriMappingConfiguration> UriMapping { get; }
        IEnumerable<Type> Modules { get; }
        void AddMessageRoute(MessageRouteConfiguration messageRoute);
        void AddUriMapping(Uri sourceUri, Uri targetUri);
        void AddModule(Type type);
    }
}