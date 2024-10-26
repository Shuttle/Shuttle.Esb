namespace Shuttle.Esb;

public interface IErrorQueueConfiguration
{
    IQueue? ErrorQueue { get; set; }
}