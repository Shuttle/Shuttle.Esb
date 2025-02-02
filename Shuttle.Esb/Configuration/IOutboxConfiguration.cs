namespace Shuttle.Esb;

public interface IOutboxConfiguration :
    IWorkQueueConfiguration,
    IErrorQueueConfiguration
{
}