using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public interface IServiceBus : IDisposable, IAsyncDisposable
    {
        bool Started { get; }
        Task<TransportMessage> Send(object message, Action<TransportMessageBuilder> builder = null);
        Task<IEnumerable<TransportMessage>> Publish(object message, Action<TransportMessageBuilder> builder = null);
        Task<IServiceBus> Start();
        Task Stop();
    }
}