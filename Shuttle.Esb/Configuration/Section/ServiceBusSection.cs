using System.Configuration;

namespace Shuttle.Esb
{
	public class ServiceBusSection : ConfigurationSection
	{
		[ConfigurationProperty("messageRoutes", IsRequired = false, DefaultValue = null)]
		public MessageRouteElementCollection MessageRoutes
		{
			get { return (MessageRouteElementCollection) this["messageRoutes"]; }
		}

		[ConfigurationProperty("queueFactories", IsRequired = false, DefaultValue = null)]
		public QueueFactoriesElement QueueFactories
		{
			get { return (QueueFactoriesElement) this["queueFactories"]; }
		}

		[ConfigurationProperty("modules", IsRequired = false, DefaultValue = null)]
		public ModulesElement Modules
		{
			get { return (ModulesElement) this["modules"]; }
		}

		[ConfigurationProperty("inbox", IsRequired = false, DefaultValue = null)]
		public InboxElement Inbox
		{
			get { return (InboxElement) this["inbox"]; }
		}

		[ConfigurationProperty("control", IsRequired = false, DefaultValue = null)]
		public ControlInboxElement ControlInbox
		{
			get { return (ControlInboxElement) this["control"]; }
		}

		[ConfigurationProperty("outbox", IsRequired = false, DefaultValue = null)]
		public OutboxElement Outbox
		{
			get { return (OutboxElement) this["outbox"]; }
		}

		[ConfigurationProperty("createQueues", IsRequired = false, DefaultValue = true)]
		public bool CreateQueues
		{
			get { return (bool) this["createQueues"]; }
		}

		[ConfigurationProperty("cacheIdentity", IsRequired = false, DefaultValue = true)]
		public bool CacheIdentity
        {
			get { return (bool) this["cacheIdentity"]; }
		}

		[ConfigurationProperty("worker", IsRequired = false)]
		public WorkerElement Worker
		{
			get { return (WorkerElement) this["worker"]; }
		}

		[ConfigurationProperty("transactionScope", IsRequired = false, DefaultValue = null)]
		public TransactionScopeElement TransactionScope
		{
			get { return (TransactionScopeElement) this["transactionScope"]; }
		}

		[ConfigurationProperty("removeMessagesNotHandled", IsRequired = false, DefaultValue = true)]
		public bool RemoveMessagesNotHandled
		{
			get { return (bool) this["removeMessagesNotHandled"]; }
		}

		[ConfigurationProperty("encryptionAlgorithm", IsRequired = false, DefaultValue = "")]
		public string EncryptionAlgorithm
		{
			get { return (string) this["encryptionAlgorithm"]; }
		}

		[ConfigurationProperty("compressionAlgorithm", IsRequired = false, DefaultValue = "")]
		public string CompressionAlgorithm
		{
			get { return (string) this["compressionAlgorithm"]; }
		}

		[ConfigurationProperty("uriResolver", IsRequired = false, DefaultValue = null)]
		public UriResolverElement UriResolver
		{
			get { return (UriResolverElement) this["uriResolver"]; }
		}
	}
}