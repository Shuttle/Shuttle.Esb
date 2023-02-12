using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
        Task<TransportMessage> Send(object message, Action<TransportMessageBuilder> builder = null);
        Task<IEnumerable<TransportMessage>> Publish(object message, Action<TransportMessageBuilder> builder = null);
    }

    public interface IHandlerContext<out T> : IHandlerContext where T : class
    {
        T Message { get; }
    }
}