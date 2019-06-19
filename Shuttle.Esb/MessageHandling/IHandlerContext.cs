using System.Threading;

namespace Shuttle.Esb
{
    public interface IHandlerContext<out T> : IMessageSender where T : class
    {
        TransportMessage TransportMessage { get; }
        T Message { get; }
        CancellationToken CancellationToken { get; }
    }
}