using System.Configuration;
using Shuttle.Core.Configuration;

namespace Shuttle.Esb
{
    public class ServiceBusSection : ConfigurationSection
    {
        private static bool _initialized;
        private static readonly object Padlock = new object();
        private static ServiceBusSection _section;

        [ConfigurationProperty("messageRoutes", IsRequired = false, DefaultValue = null)]
        public MessageRouteElementCollection MessageRoutes => (MessageRouteElementCollection) this["messageRoutes"];

        [ConfigurationProperty("queueFactories", IsRequired = false, DefaultValue = null)]
        public QueueFactoriesElement QueueFactories => (QueueFactoriesElement) this["queueFactories"];

        [ConfigurationProperty("inbox", IsRequired = false, DefaultValue = null)]
        public InboxElement Inbox => (InboxElement) this["inbox"];

        [ConfigurationProperty("control", IsRequired = false, DefaultValue = null)]
        public ControlInboxElement ControlInbox => (ControlInboxElement) this["control"];

        [ConfigurationProperty("outbox", IsRequired = false, DefaultValue = null)]
        public OutboxElement Outbox => (OutboxElement) this["outbox"];

        [ConfigurationProperty("createQueues", IsRequired = false, DefaultValue = true)]
        public bool CreateQueues => (bool) this["createQueues"];

        [ConfigurationProperty("cacheIdentity", IsRequired = false, DefaultValue = true)]
        public bool CacheIdentity => (bool) this["cacheIdentity"];

        [ConfigurationProperty("registerHandlers", IsRequired = false, DefaultValue = true)]
        public bool RegisterHandlers => (bool) this["registerHandlers"];

        [ConfigurationProperty("worker", IsRequired = false)]
        public WorkerElement Worker => (WorkerElement) this["worker"];

        [ConfigurationProperty("removeMessagesNotHandled", IsRequired = false, DefaultValue = false)]
        public bool RemoveMessagesNotHandled => (bool) this["removeMessagesNotHandled"];

        [ConfigurationProperty("encryptionAlgorithm", IsRequired = false, DefaultValue = "")]
        public string EncryptionAlgorithm => (string) this["encryptionAlgorithm"];

        [ConfigurationProperty("compressionAlgorithm", IsRequired = false, DefaultValue = "")]
        public string CompressionAlgorithm => (string) this["compressionAlgorithm"];

        [ConfigurationProperty("uriResolver", IsRequired = false, DefaultValue = null)]
        public UriResolverElement UriResolver => (UriResolverElement) this["uriResolver"];

        public static ServiceBusSection Get()
        {
            lock (Padlock)
            {
                if (!_initialized)
                {
                    _section =
                        ConfigurationSectionProvider.Open<ServiceBusSection>("shuttle", "serviceBus");

                    _initialized = true;
                }

                return _section;
            }
        }
    }
}