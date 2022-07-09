namespace Shuttle.Esb
{
    public interface IInboxQueueConfiguration : IWorkProcessorConfiguration
    {
        IQueue DeferredQueue { get; set; }
        DeferredMessageProcessor DeferredMessageProcessor { get; set; }
    }
}