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
        private static readonly object Padlock = new object();
        private readonly List<ICompressionAlgorithm> _compressionAlgorithms = new List<ICompressionAlgorithm>();
        private readonly List<IEncryptionAlgorithm> _encryptionAlgorithms = new List<IEncryptionAlgorithm>();
        private ITransactionScopeConfiguration _transactionScope;
        private IComponentResolver _resolver;

        public ServiceBusConfiguration()
        {
            RegisterHandlers = true;
        }

        public static ServiceBusSection ServiceBusSection
        {
            get
            {
                return _serviceBusSection ??
                       Synchronised(
                           () =>
                               _serviceBusSection =
                                   ConfigurationSectionProvider.Open<ServiceBusSection>("shuttle", "serviceBus"));
            }
        }

        public IInboxQueueConfiguration Inbox { get; set; }
        public IControlInboxQueueConfiguration ControlInbox { get; set; }
        public IOutboxQueueConfiguration Outbox { get; set; }
        public IWorkerConfiguration Worker { get; set; }

        public ITransactionScopeConfiguration TransactionScope
        {
            get
            {
                return _transactionScope ?? Synchronised(() => _transactionScope = new TransactionScopeConfiguration());
            }
            set { _transactionScope = value; }
        }

        public bool CreateQueues { get; set; }
        public bool CacheIdentity { get; set; }
        public bool RegisterHandlers { get; set; }

        public IComponentResolver Resolver
        {
            get
            {
                if (_resolver == null)
                {
                    throw new InvalidOperationException(string.Format(InfrastructureResources.NullDependencyException, typeof(IComponentResolver).FullName));
                }

                return _resolver;
            }
        }

        public IServiceBusConfiguration Assign(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, "resolver");

            _resolver = resolver;

            return this;
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

        public IEncryptionAlgorithm FindEncryptionAlgorithm(string name)
        {
            return
                _encryptionAlgorithms.Find(
                    algorithm => algorithm.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public void AddEncryptionAlgorithm(IEncryptionAlgorithm algorithm)
        {
            Guard.AgainstNull(algorithm, "algorithm");

            _encryptionAlgorithms.Add(algorithm);
        }

        public ICompressionAlgorithm FindCompressionAlgorithm(string name)
        {
            return
                _compressionAlgorithms.Find(
                    algorithm => algorithm.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
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

        public void Invariant()
        {
            Guard.Against<WorkerException>(IsWorker && !HasInbox,
                EsbResources.WorkerRequiresInboxException);

            if (HasInbox)
            {
                Guard.Against<EsbConfigurationException>(string.IsNullOrEmpty(Inbox.WorkQueueUri),
                    string.Format(EsbResources.RequiredQueueUriMissing, "Inbox.WorkQueueUri"));

                Guard.Against<EsbConfigurationException>(string.IsNullOrEmpty(Inbox.ErrorQueueUri),
                    string.Format(EsbResources.RequiredQueueUriMissing, "Inbox.ErrorQueueUri"));
            }

            if (HasOutbox)
            {
                Guard.Against<EsbConfigurationException>(string.IsNullOrEmpty(Outbox.WorkQueueUri),
                    string.Format(EsbResources.RequiredQueueUriMissing, "Outbox.WorkQueueUri"));

                Guard.Against<EsbConfigurationException>(string.IsNullOrEmpty(Outbox.ErrorQueueUri),
                    string.Format(EsbResources.RequiredQueueUriMissing, "Outbox.ErrorQueueUri"));
            }

            if (HasControlInbox)
            {
                Guard.Against<EsbConfigurationException>(string.IsNullOrEmpty(ControlInbox.WorkQueueUri),
                    string.Format(EsbResources.RequiredQueueUriMissing, "ControlInbox.WorkQueueUri"));

                Guard.Against<EsbConfigurationException>(string.IsNullOrEmpty(ControlInbox.ErrorQueueUri),
                    string.Format(EsbResources.RequiredQueueUriMissing, "ControlInbox.ErrorQueueUri"));
            }
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