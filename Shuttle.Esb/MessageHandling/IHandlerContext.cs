using System.Threading;

namespace Shuttle.Esb
{
    public enum ExceptionHandling
    {
        Default = 0,
        Retry = 1,
        Block = 2,
        Poison = 3
    }

    public interface IHandlerContext
    {
        TransportMessage TransportMessage { get; }
        CancellationToken CancellationToken { get; }
        ExceptionHandling ExceptionHandling { get; }
    }

    public interface IHandlerContext<out T> : IHandlerContext, IMessageSender where T : class
    {
        T Message { get; }
    }
}