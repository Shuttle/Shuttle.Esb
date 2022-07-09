namespace Shuttle.Esb
{
    public interface IOutboxQueueConfiguration :
        IWorkQueueConfiguration,
        IErrorQueueConfiguration
    {
    }
}