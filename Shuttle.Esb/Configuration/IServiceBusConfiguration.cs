using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public interface IServiceBusConfiguration
    {
        bool IsWorker { get; }
        bool ShouldRemoveMessagesNotHandled { get; }
        bool ShouldRemoveCorruptMessages { get; }

        IControlInboxQueueConfiguration ControlInbox { get; }
        IInboxQueueConfiguration Inbox { get; }
        IOutboxQueueConfiguration Outbox { get; }
        IWorkerConfiguration Worker { get; }

        string EncryptionAlgorithm { get; }
        string CompressionAlgorithm { get; }

        bool ShouldCreateQueues { get; }
        bool ShouldCacheIdentity { get; }
        bool ShouldAddMessageHandlers { get; }

        IEnumerable<MessageRouteConfiguration> MessageRoutes { get; }
        IEnumerable<UriMappingConfiguration> UriMapping { get; }
        void AddMessageRoute(MessageRouteConfiguration messageRoute);
        void AddUriMapping(Uri sourceUri, Uri targetUri);
    }
}