using System;
using System.Collections.Generic;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class ServiceBusOptions
    {
        public const string SectionName = "Shuttle:ServiceBus";

        public static readonly IEnumerable<TimeSpan> DefaultDurationToIgnoreOnFailure = new List<TimeSpan>
        {
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMinutes(2),
            TimeSpan.FromMinutes(5),
        }.AsReadOnly();

        public static readonly IEnumerable<TimeSpan> DefaultDurationToSleepWhenIdle = new List<TimeSpan>
        {
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5)
        }.AsReadOnly(); 
        
        public bool CreatePhysicalQueues { get; set; } = true;
        public bool CacheIdentity { get; set; } = true;
        public bool AddMessageHandlers { get; set; } = true;
        public bool RemoveMessagesNotHandled { get; set; } = false;
        public bool RemoveCorruptMessages { get; set; } = false;
        public string EncryptionAlgorithm { get; set; }
        public string CompressionAlgorithm { get; set; }
        public bool Asynchronous { get; set; }

        public InboxOptions Inbox { get; set; } = new InboxOptions();
        public OutboxOptions Outbox { get; set; } = new OutboxOptions();
        public List<MessageRouteOptions> MessageRoutes { get; set; } = new List<MessageRouteOptions>();
        public List<UriMappingOptions> UriMappings { get; set; } = new List<UriMappingOptions>();
        public SubscriptionOptions Subscription { get; set; } = new SubscriptionOptions();
        public IdempotenceOptions Idempotence { get; set; } = new IdempotenceOptions();
        public ProcessorThreadOptions ProcessorThread { get; set; } = new ProcessorThreadOptions();
    }
}