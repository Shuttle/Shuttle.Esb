using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public interface IOutboxQueueConfiguration :
        IWorkQueueConfiguration,
        IErrorQueueConfiguration,
        IMessageFailureConfiguration,
        IThreadActivityConfiguration
    {
    }
}