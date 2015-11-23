using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
    public interface IHandlerContext<out T> : IMessageSender where T : class
    {
        TransportMessage TransportMessage { get; }
        T Message { get; }
        IThreadState ActiveState { get; }
        IServiceBusConfiguration Configuration { get; }
    }
}