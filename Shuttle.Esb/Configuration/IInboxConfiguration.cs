namespace Shuttle.Esb
{
    public interface IInboxConfiguration : IWorkProcessorConfiguration
    {
        bool Distribute { get; set; }
        int DistributeSendCount { get; set; }
        bool HasDeferredBrokerEndpoint { get; }
        IBrokerEndpoint DeferredBrokerEndpoint { get; set; }
        string DeferredUri { get; set; }
        DeferredMessageProcessor DeferredMessageProcessor { get; set; }
    }
}