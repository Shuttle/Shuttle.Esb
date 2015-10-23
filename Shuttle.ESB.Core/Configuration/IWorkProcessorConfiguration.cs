namespace Shuttle.ESB.Core
{
    public interface IWorkProcessorConfiguration:
        IWorkQueueConfiguration,
        IErrorQueueConfiguration,
        IMessageFailureConfiguration,
        IThreadActivityConfiguration
    {
    }
}