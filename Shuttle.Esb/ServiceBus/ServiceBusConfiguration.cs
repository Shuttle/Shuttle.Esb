using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class ServiceBusConfiguration : IServiceBusConfiguration
    {
        public static readonly TimeSpan[] DefaultDurationToIgnoreOnFailure =
        {
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(10),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(60)
        };

        public static readonly TimeSpan[] DefaultDurationToSleepWhenIdle =
            (TimeSpan[])
                new StringDurationArrayConverter()
                    .ConvertFrom("250ms*4,500ms*2,1s");


        private static ServiceBusSection _serviceBusSection;
        private readonly List<ICompressionAlgorithm> _compressionAlgorithms = new List<ICompressionAlgorithm>();
        private readonly List<IEncryptionAlgorithm> _encryptionAlgorithms = new List<IEncryptionAlgorithm>();
        private IMessageHandlerFactory _messageHandlerFactory;
        private IMessageHandlerInvoker _messageHandlerInvoker;
        private IMessageHandlingAssessor _messageHandlingAssessor;
        private IMessageRouteProvider _messageRouteProvider;
        private IIdentityProvider _identityProvider;
        private IPipelineFactory _pipelineFactory;
        private IServiceBusPolicy _policy;
        private IQueueManager _queueManager;

        private ISerializer _serializer;
        private ISubscriptionManager _subscriptionManager;
        private IThreadActivityFactory _threadActivityFactory;
        private ITransactionScopeConfiguration _transactionScope;
        private ITransactionScopeFactory _transactionScopeFactory;
        private IWorkerAvailabilityManager _workerAvailabilityManager;
        private IUriResolver _uriResolver;
        private static readonly object Padlock = new object();

        public ServiceBusConfiguration()
        {
            Modules = new ModuleCollection();
        }

        public static ServiceBusSection ServiceBusSection
        {
            get
            {
                return _serviceBusSection ??
                       Synchronised(
                           () => _serviceBusSection = ConfigurationSectionProvider.Open<ServiceBusSection>("shuttle", "serviceBus"));
            }
        }

        public ISerializer Serializer
        {
            get { return _serializer ?? Synchronised(() => _serializer = new DefaultSerializer()); }
            set
            {
                Guard.AgainstNull(value, "serializer");

                if (value.Equals(Serializer))
                {
                    return;
                }

                _serializer = value;
            }
        }

        public IInboxQueueConfiguration Inbox { get; set; }
        public IControlInboxQueueConfiguration ControlInbox { get; set; }
        public IOutboxQueueConfiguration Outbox { get; set; }
        public IWorkerConfiguration Worker { get; set; }

        public ITransactionScopeConfiguration TransactionScope
        {
            get { return _transactionScope ?? Synchronised(() => _transactionScope = new TransactionScopeConfiguration()); }
            set { _transactionScope = value; }
        }

        public bool CreateQueues { get; set; }
        public bool CacheIdentity { get; set; }
        public bool RegisterHandlers { get; set; }

        public IUriResolver UriResolver
        {
            get { return _uriResolver ?? Synchronised(() => _uriResolver = new DefaultUriResolver()); }
            set { _uriResolver = value; }
        }

        public IQueueManager QueueManager
        {
            get { return _queueManager ?? Synchronised(() => _queueManager = new QueueManager()); }
            set { _queueManager = value; }
        }

        public IIdempotenceService IdempotenceService { get; set; }

        public ModuleCollection Modules { get; private set; }

        public IMessageHandlerFactory MessageHandlerFactory
        {
            get
            {
                return _messageHandlerFactory ?? Synchronised(() => _messageHandlerFactory = new DefaultMessageHandlerFactory());
            }
            set { _messageHandlerFactory = value; }
        }

        public IMessageHandlerInvoker MessageHandlerInvoker
        {
            get
            {
                return _messageHandlerInvoker ?? Synchronised(() => _messageHandlerInvoker = new DefaultMessageHandlerInvoker());
            }
            set { _messageHandlerInvoker = value; }
        }

        public IMessageHandlingAssessor MessageHandlingAssessor
        {
            get
            {
                return _messageHandlingAssessor ??
                       Synchronised(() => _messageHandlingAssessor = new DefaultMessageHandlingAssessor());
            }
            set { _messageHandlingAssessor = value; }
        }

        public IMessageRouteProvider MessageRouteProvider
        {
            get
            {
                return _messageRouteProvider ?? Synchronised(() => _messageRouteProvider = new DefaultMessageRouteProvider());
            }
            set { _messageRouteProvider = value; }
        }

        public IIdentityProvider IdentityProvider
        {
            get
            {
                return _identityProvider ?? Synchronised(() => _identityProvider = new DefaultIdentityProvider());
            }
            set { _identityProvider = value; }
        }

        public IServiceBusPolicy Policy
        {
            get { return _policy ?? Synchronised(() => _policy = new DefaultServiceBusPolicy()); }
            set { _policy = value; }
        }

        public IThreadActivityFactory ThreadActivityFactory
        {
            get
            {
                return _threadActivityFactory ?? Synchronised(() => _threadActivityFactory = new DefaultThreadActivityFactory());
            }
            set { _threadActivityFactory = value; }
        }

        public bool HasIdempotenceService
        {
            get { return IdempotenceService != null; }
        }

        public bool HasSubscriptionManager
        {
            get { return _subscriptionManager != null; }
        }

        public ISubscriptionManager SubscriptionManager
        {
            get
            {
                if (!HasSubscriptionManager)
                {
                    throw new SubscriptionManagerException(EsbResources.NoSubscriptionManager);
                }

                return _subscriptionManager;
            }
            set { _subscriptionManager = value; }
        }

        public bool HasInbox
        {
            get { return Inbox != null; }
        }

        public bool HasOutbox
        {
            get { return Outbox != null; }
        }

        public bool HasControlInbox
        {
            get { return ControlInbox != null; }
        }

        public bool RemoveMessagesNotHandled { get; set; }
        public string EncryptionAlgorithm { get; set; }
        public string CompressionAlgorithm { get; set; }

        public IWorkerAvailabilityManager WorkerAvailabilityManager
        {
            get
            {
                return _workerAvailabilityManager ??
                       Synchronised(() => _workerAvailabilityManager = new WorkerAvailabilityManager());
            }
            set { _workerAvailabilityManager = value; }
        }

        public IPipelineFactory PipelineFactory
        {
            get { return _pipelineFactory ?? Synchronised(() => _pipelineFactory = new DefaultPipelineFactory()); }
            set { _pipelineFactory = value; }
        }

        public ITransactionScopeFactory TransactionScopeFactory
        {
            get
            {
                return _transactionScopeFactory ??
                       Synchronised(() => _transactionScopeFactory = new DefaultTransactionScopeFactory(TransactionScope.Enabled, TransactionScope.IsolationLevel, TimeSpan.FromSeconds(TransactionScope.TimeoutSeconds)));
            }
            set { _transactionScopeFactory = value; }
        }

        public IEncryptionAlgorithm FindEncryptionAlgorithm(string name)
        {
            return
                _encryptionAlgorithms.Find(algorithm => algorithm.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public void AddEncryptionAlgorithm(IEncryptionAlgorithm algorithm)
        {
            Guard.AgainstNull(algorithm, "algorithm");

            _encryptionAlgorithms.Add(algorithm);
        }

        public ICompressionAlgorithm FindCompressionAlgorithm(string name)
        {
            return
                _compressionAlgorithms.Find(algorithm => algorithm.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public void AddCompressionAlgorithm(ICompressionAlgorithm algorithm)
        {
            Guard.AgainstNull(algorithm, "algorithm");

            _compressionAlgorithms.Add(algorithm);
        }

        public bool IsWorker
        {
            get { return Worker != null; }
        }

        private static T Synchronised<T>(Func<T> f)
        {
            lock (Padlock)
            {
                return f.Invoke();
            }
        }
    }
}