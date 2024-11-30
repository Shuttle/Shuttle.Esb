using System;
using System.Collections.Generic;
using Shuttle.Core.Threading;

namespace Shuttle.Esb;

public class ServiceBusOptions
{
    public const string SectionName = "Shuttle:ServiceBus";

    public static readonly IEnumerable<TimeSpan> DefaultDurationToIgnoreOnFailure = new List<TimeSpan>
    {
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(5)
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

    public bool AddMessageHandlers { get; set; } = true;
    public bool CacheIdentity { get; set; } = true;
    public string CompressionAlgorithm { get; set; } = string.Empty;
    public bool CreatePhysicalQueues { get; set; } = true;
    public string EncryptionAlgorithm { get; set; } = string.Empty;
    public InboxOptions Inbox { get; set; } = new();
    public List<MessageRouteOptions> MessageRoutes { get; set; } = new();
    public OutboxOptions Outbox { get; set; } = new();
    public ProcessorThreadOptions ProcessorThread { get; set; } = new();
    public bool RemoveCorruptMessages { get; set; } = false;
    public bool RemoveMessagesNotHandled { get; set; } = false;
    public SubscriptionOptions Subscription { get; set; } = new();
    public List<UriMappingOptions> UriMappings { get; set; } = new();
}