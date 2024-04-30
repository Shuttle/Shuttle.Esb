using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public interface IServiceBus : IDisposable, IAsyncDisposable
    {
        bool Started { get; }
        bool Asynchronous { get; }
        TransportMessage Send(object message, Action<TransportMessageBuilder> builder = null);
        Task<TransportMessage> SendAsync(object message, Action<TransportMessageBuilder> builder = null);
        IEnumerable<TransportMessage> Publish(object message, Action<TransportMessageBuilder> builder = null);
        Task<IEnumerable<TransportMessage>> PublishAsync(object message, Action<TransportMessageBuilder> builder = null);
        IServiceBus Start();
        Task<IServiceBus> StartAsync();
        Task StopAsync();
    }
}