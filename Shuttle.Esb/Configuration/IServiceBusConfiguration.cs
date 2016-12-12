using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public interface IServiceBusConfiguration
	{
	    IComponentResolver Resolver { get; }
        IServiceBusConfiguration Assign(IComponentResolver resolver);

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

	    IEncryptionAlgorithm FindEncryptionAlgorithm(string name);
		void AddEncryptionAlgorithm(IEncryptionAlgorithm algorithm);

		ICompressionAlgorithm FindCompressionAlgorithm(string name);
		void AddCompressionAlgorithm(ICompressionAlgorithm algorithm);

        IEnumerable<Type> QueueFactoryTypes { get; }
	    void AddQueueFactoryType(Type type);

        bool ScanForQueueFactories { get; set; }

        IEnumerable<MessageRouteConfiguration> MessageRoutes { get; }
        void AddMessageRoute(MessageRouteConfiguration messageRoute);

        IEnumerable<UriMappingConfiguration> UriMapping { get; }
        void AddUriMapping(Uri sourceUri, Uri targetUri);
	}
}