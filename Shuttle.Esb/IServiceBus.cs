using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Esb;

public interface IServiceBus : IDisposable, IAsyncDisposable
{
    bool Started { get; }
    Task<IEnumerable<TransportMessage>> PublishAsync(object message, Action<TransportMessageBuilder>? builder = null);
    Task<TransportMessage> SendAsync(object message, Action<TransportMessageBuilder>? builder = null);
    Task<IServiceBus> StartAsync();
    Task StopAsync();
}