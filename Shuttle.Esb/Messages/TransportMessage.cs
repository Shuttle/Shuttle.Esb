using System;
using System.Collections.Generic;

namespace Shuttle.Esb;

[Serializable]
public class TransportMessage
{
    public string AssemblyQualifiedName { get; set; } = string.Empty;
    public string CompressionAlgorithm { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string EncryptionAlgorithm { get; set; } = string.Empty;
    public DateTimeOffset ExpiryDate { get; set; } = DateTimeOffset.MaxValue;
    public List<string> FailureMessages { get; set; } = new();
    public List<TransportHeader> Headers { get; set; } = new();
    public DateTimeOffset IgnoreTillDate { get; set; } = DateTimeOffset.MinValue;
    public byte[] Message { get; set; } = null!;
    public Guid MessageId { get; set; } = Guid.NewGuid();

    public Guid MessageReceivedId { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public string PrincipalIdentityName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string RecipientInboxWorkQueueUri { get; set; } = string.Empty;
    public DateTimeOffset SendDate { get; set; } = DateTimeOffset.UtcNow;
    public string SenderInboxWorkQueueUri { get; set; } = string.Empty;
}