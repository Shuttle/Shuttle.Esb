using System.Reflection;
using System.Threading;

namespace Shuttle.Esb
{
    public interface IHandlerContext
    {
        TransportMessage TransportMessage { get; }
        CancellationToken CancellationToken { get; }
        ExceptionHandling ExceptionHandler { get; }
    }

    public interface IHandlerContext<out T> : IHandlerContext, IMessageSender where T : class
    {
        T Message { get; }
    }
}