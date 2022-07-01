using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public interface IOutboxConfiguration :
        IWorkConfiguration,
        IErrorConfiguration,
        IMessageFailureConfiguration,
        IThreadActivityConfiguration
    {
    }
}