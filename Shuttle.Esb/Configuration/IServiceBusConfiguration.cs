using System;
using System.Collections.Generic;
using Shuttle.Core.Compression;
using Shuttle.Core.Encryption;

namespace Shuttle.Esb
{
    public interface IServiceBusConfiguration
    {
        bool HasInbox { get; }
        bool HasControlInbox { get; }
        bool HasOutbox { get; }

        bool IsWorker { get; }
        bool RemoveMessagesNotHandled { get; }
        bool RemoveCorruptMessages { get; }

        IControlInboxQueueConfiguration ControlInbox { get; }
        IInboxQueueConfiguration Inbox { get; }
        IOutboxQueueConfiguration Outbox { get; }
        IWorkerConfiguration Worker { get; }

        string EncryptionAlgorithm { get; }
        string CompressionAlgorithm { get; }

        bool CreateQueues { get; }
        bool CacheIdentity { get; }
        bool RegisterHandlers { get; }

        IEnumerable<Type> QueueFactoryTypes { get; }

        bool ScanForQueueFactories { get; set; }

        IEnumerable<MessageRouteConfiguration> MessageRoutes { get; }

        IEnumerable<UriMappingConfiguration> UriMapping { get; }

        IEncryptionAlgorithm FindEncryptionAlgorithm(string name);
        void AddEncryptionAlgorithm(IEncryptionAlgorithm algorithm);

        ICompressionAlgorithm FindCompressionAlgorithm(string name);
        void AddCompressionAlgorithm(ICompressionAlgorithm algorithm);
        void AddQueueFactoryType(Type type);
        void AddMessageRoute(MessageRouteConfiguration messageRoute);
        void AddUriMapping(Uri sourceUri, Uri targetUri);
    }
}