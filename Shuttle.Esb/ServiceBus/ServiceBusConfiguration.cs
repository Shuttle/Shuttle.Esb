using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;
using Shuttle.Core.TimeSpanTypeConverters;

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

        private static readonly object Padlock = new object();
        private readonly List<ICompressionAlgorithm> _compressionAlgorithms = new List<ICompressionAlgorithm>();
        private readonly List<IEncryptionAlgorithm> _encryptionAlgorithms = new List<IEncryptionAlgorithm>();
        private readonly List<MessageRouteConfiguration> _messageRoutes = new List<MessageRouteConfiguration>();
        private readonly List<Type> _queueFactoryTypes = new List<Type>();
        private readonly List<UriMappingConfiguration> _uriMapping = new List<UriMappingConfiguration>();

        public ServiceBusConfiguration()
        {
            ScanForQueueFactories = true;
            CreateQueues = true;
            CacheIdentity = true;
            RegisterHandlers = true;
            RemoveMessagesNotHandled = false;
        }

        public IInboxQueueConfiguration Inbox { get; set; }
        public IControlInboxQueueConfiguration ControlInbox { get; set; }
        public IOutboxQueueConfiguration Outbox { get; set; }
        public IWorkerConfiguration Worker { get; set; }

        public bool CreateQueues { get; set; }
        public bool CacheIdentity { get; set; }
        public bool RegisterHandlers { get; set; }

        public bool HasInbox => Inbox != null;

        public bool HasOutbox => Outbox != null;

        public bool HasControlInbox => ControlInbox != null;

        public bool RemoveMessagesNotHandled { get; set; }
        public bool RemoveCorruptMessages { get; set; }
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
            Guard.AgainstNull(algorithm, nameof(algorithm));

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
            Guard.AgainstNull(algorithm, nameof(algorithm));

            _compressionAlgorithms.Add(algorithm);
        }

        public IEnumerable<Type> QueueFactoryTypes => new ReadOnlyCollection<Type>(_queueFactoryTypes);

        public void AddQueueFactoryType(Type type)
        {
            Guard.AgainstNull(type, nameof(type));

            _queueFactoryTypes.Add(type);
        }

        public bool ScanForQueueFactories { get; set; }

        public IEnumerable<MessageRouteConfiguration> MessageRoutes =>
            new ReadOnlyCollection<MessageRouteConfiguration>(_messageRoutes);

        public void AddMessageRoute(MessageRouteConfiguration messageRoute)
        {
            Guard.AgainstNull(messageRoute, nameof(messageRoute));

            _messageRoutes.Add(messageRoute);
        }

        public IEnumerable<UriMappingConfiguration> UriMapping =>
            new ReadOnlyCollection<UriMappingConfiguration>(_uriMapping);

        public void AddUriMapping(Uri sourceUri, Uri targetUri)
        {
            _uriMapping.Add(new UriMappingConfiguration(sourceUri, targetUri));
        }

        public bool IsWorker => Worker != null;
    }
}