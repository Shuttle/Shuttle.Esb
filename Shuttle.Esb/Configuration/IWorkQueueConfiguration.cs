namespace Shuttle.Esb;

public interface IWorkQueueConfiguration
{
    IQueue? WorkQueue { get; set; }
}