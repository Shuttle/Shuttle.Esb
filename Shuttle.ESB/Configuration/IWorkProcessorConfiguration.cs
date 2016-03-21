using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public interface IWorkProcessorConfiguration:
        IWorkQueueConfiguration,
        IErrorQueueConfiguration,
        IMessageFailureConfiguration,
        IThreadActivityConfiguration
    {
    }
}