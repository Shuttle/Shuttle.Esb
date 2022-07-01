using System;
using System.Collections.Generic;
using Shuttle.Core.Compression;
using Shuttle.Core.Encryption;

namespace Shuttle.Esb
{
    public interface IServiceBusConfiguration
    {
        bool HasInbox { get; }
        bool HasControl { get; }
        bool HasOutbox { get; }

        bool IsWorker { get; }
        bool RemoveMessagesNotHandled { get; }
        bool RemoveCorruptMessages { get; }

        IControlConfiguration Control { get; }
        IInboxConfiguration Inbox { get; }
        IOutboxConfiguration Outbox { get; }
        IWorkerConfiguration Worker { get; }

        string EncryptionAlgorithm { get; }
        string CompressionAlgorithm { get; }

        bool CreateBrokerEndpoints { get; }
        bool CacheIdentity { get; }
        bool RegisterHandlers { get; }

        IEnumerable<Type> BrokerEndpointFactoryTypes { get; }

        bool ScanForBrokerEndpointFactories { get; }

        IEnumerable<MessageRouteConfiguration> MessageRoutes { get; }

        IEnumerable<UriMappingConfiguration> UriMapping { get; }

        IEncryptionAlgorithm FindEncryptionAlgorithm(string name);
        void AddEncryptionAlgorithm(IEncryptionAlgorithm algorithm);

        ICompressionAlgorithm FindCompressionAlgorithm(string name);
        void AddCompressionAlgorithm(ICompressionAlgorithm algorithm);
        void AddBrokerEndpointFactoryType(Type type);
        void AddMessageRoute(MessageRouteConfiguration messageRoute);
        void AddUriMapping(Uri sourceUri, Uri targetUri);
    }
}