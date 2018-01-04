using System;
using System.Collections.Generic;
using Shuttle.Core.Compression;
using Shuttle.Core.Container;
using Shuttle.Core.Encryption;
using Shuttle.Core.Transactions;

namespace Shuttle.Esb
{
    public interface IServiceBusConfiguration
    {
        IComponentResolver Resolver { get; }

        bool HasInbox { get; }
        bool HasControlInbox { get; }
        bool HasOutbox { get; }

        bool IsWorker { get; }
        bool RemoveMessagesNotHandled { get; set; }

        IControlInboxQueueConfiguration ControlInbox { get; set; }
        IInboxQueueConfiguration Inbox { get; set; }
        IOutboxQueueConfiguration Outbox { get; set; }
        IWorkerConfiguration Worker { get; set; }

        string EncryptionAlgorithm { get; set; }
        string CompressionAlgorithm { get; set; }

        ITransactionScopeConfiguration TransactionScope { get; set; }

        bool CreateQueues { get; set; }
        bool CacheIdentity { get; set; }
        bool RegisterHandlers { get; set; }

        IEnumerable<Type> QueueFactoryTypes { get; }

        bool ScanForQueueFactories { get; set; }

        IEnumerable<MessageRouteConfiguration> MessageRoutes { get; }

        IEnumerable<UriMappingConfiguration> UriMapping { get; }
        IServiceBusConfiguration Assign(IComponentResolver resolver);

        IEncryptionAlgorithm FindEncryptionAlgorithm(string name);
        void AddEncryptionAlgorithm(IEncryptionAlgorithm algorithm);

        ICompressionAlgorithm FindCompressionAlgorithm(string name);
        void AddCompressionAlgorithm(ICompressionAlgorithm algorithm);
        void AddQueueFactoryType(Type type);
        void AddMessageRoute(MessageRouteConfiguration messageRoute);
        void AddUriMapping(Uri sourceUri, Uri targetUri);
    }
}