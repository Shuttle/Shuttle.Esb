namespace Shuttle.Esb;

public class InboxConfiguration : IInboxConfiguration
{
    public IQueue? WorkQueue { get; set; }
    public IQueue? ErrorQueue { get; set; }
    public IQueue? DeferredQueue { get; set; } 
}