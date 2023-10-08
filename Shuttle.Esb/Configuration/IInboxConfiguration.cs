namespace Shuttle.Esb
{
    public interface IInboxConfiguration : IWorkProcessorConfiguration
    {
        IQueue DeferredQueue { get; set; }
    }
}