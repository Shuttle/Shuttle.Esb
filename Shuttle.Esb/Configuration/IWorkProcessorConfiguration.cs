using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public interface IWorkProcessorConfiguration :
        IWorkQueueConfiguration,
        IErrorQueueConfiguration,
        IMessageFailureConfiguration,
        IThreadActivityConfiguration
    {
    }
}