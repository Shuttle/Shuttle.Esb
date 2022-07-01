using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public interface IWorkProcessorConfiguration :
        IWorkConfiguration,
        IErrorConfiguration,
        IMessageFailureConfiguration,
        IThreadActivityConfiguration
    {
    }
}