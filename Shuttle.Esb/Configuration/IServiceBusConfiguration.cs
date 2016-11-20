using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public interface IServiceBusConfiguration
	{
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
	    void Invariant();
	}
}