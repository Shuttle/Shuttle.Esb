using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    [Serializable]
    public class TransportMessage
    {
        public byte[] Message { get; set; }

        public Guid MessageReceivedId { get; set; }
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public string CorrelationId { get; set; }
        public string SenderInboxWorkQueueUri { get; set; }
        public string RecipientInboxWorkQueueUri { get; set; }
        public string PrincipalIdentityName { get; set; }
        public DateTime IgnoreTillDate { get; set; } = DateTime.MinValue;
        public DateTime SendDate { get; set; } = DateTime.Now;
        public DateTime ExpiryDate { get; set; } = DateTime.MaxValue;
        public int Priority { get; set; }
        public List<string> FailureMessages { get; set; } = new List<string>();
        public string MessageType { get; set; }
        public string AssemblyQualifiedName { get; set; }
        public string EncryptionAlgorithm { get; set; }
        public string CompressionAlgorithm { get; set; }
        public List<TransportHeader> Headers { get; set; } = new List<TransportHeader>();
    }
}