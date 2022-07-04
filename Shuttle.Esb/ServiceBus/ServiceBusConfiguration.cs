using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;

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
        {
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromSeconds(1)
        };

        private readonly List<ICompressionAlgorithm> _compressionAlgorithms = new List<ICompressionAlgorithm>();
        private readonly List<IEncryptionAlgorithm> _encryptionAlgorithms = new List<IEncryptionAlgorithm>();
        private readonly List<MessageRouteConfiguration> _messageRoutes = new List<MessageRouteConfiguration>();
        private readonly List<Type> _queueFactoryTypes = new List<Type>();
        private readonly List<UriMappingConfiguration> _uriMapping = new List<UriMappingConfiguration>();

        public ServiceBusConfiguration()
        {
            ShouldCreateQueues = true;
            ShouldCacheIdentity = true;
            ShouldAddMessageHandlers = true;
            ShouldRemoveMessagesNotHandled = false;
        }

        public IInboxQueueConfiguration Inbox { get; set; }
        public IControlInboxQueueConfiguration ControlInbox { get; set; }
        public IOutboxQueueConfiguration Outbox { get; set; }
        public IWorkerConfiguration Worker { get; set; }

        public bool ShouldCreateQueues { get; set; }
        public bool ShouldCacheIdentity { get; set; }
        public bool ShouldAddMessageHandlers { get; set; }

        public bool HasInbox => Inbox != null;

        public bool HasOutbox => Outbox != null;

        public bool HasControlInbox => ControlInbox != null;

        public bool ShouldRemoveMessagesNotHandled { get; set; }
        public bool ShouldRemoveCorruptMessages { get; set; }
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