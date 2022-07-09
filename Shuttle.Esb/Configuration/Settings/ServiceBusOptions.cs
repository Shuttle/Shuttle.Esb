using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public class ServiceBusOptions
    {
        public const string SectionName = "Shuttle:ServiceBus";

        public static readonly IEnumerable<TimeSpan> DefaultDurationToIgnoreOnFailure = new List<TimeSpan>
        {
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(10),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(60)
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
        
        public bool CreateQueues { get; set; } = true;
        public bool CacheIdentity { get; set; } = true;
        public bool RegisterHandlers { get; set; } = true;
        public bool RemoveMessagesNotHandled { get; set; } = false;
        public bool RemoveCorruptMessages { get; set; } = true;
        public string EncryptionAlgorithm { get; set; }
        public string CompressionAlgorithm { get; set; }

        public InboxOptions Inbox { get; set; } = new InboxOptions();
        public OutboxOptions Outbox { get; set; } = new OutboxOptions();
        public ControlInboxOptions ControlInbox { get; set; } = new ControlInboxOptions();
        public List<MessageRouteOptions> MessageRoutes { get; set; } = new List<MessageRouteOptions>();
        public WorkerOptions Worker { get; set; } = new WorkerOptions();
    }
}