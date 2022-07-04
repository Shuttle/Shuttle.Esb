using System.Configuration;

namespace Shuttle.Esb
{
    public class ServiceBusSettings
    {
        public bool CreateQueues { get; set; } = true;
        public bool CacheIdentity { get; set; } = true;
        public bool RegisterHandlers { get; set; } = true;
        public bool RemoveMessagesNotHandled { get; set; } = false;
        public bool RemoveCorruptMessages { get; set; } = true;
        public string EncryptionAlgorithm { get; set; }
        public string CompressionAlgorithm { get; set; }

        public InboxSettings Inbox { get; set; }
        public OutboxSettings Outbox { get; set; }
        public ControlInboxSettings ControlInbox { get; set; }
        public MessageRouteSettings[] MessageRoutes { get; set; }
        public WorkerSettings Worker { get; set; }
    }
}