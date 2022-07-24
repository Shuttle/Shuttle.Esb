using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public interface IServiceBus : IDisposable
    {
        bool Started { get; }
        TransportMessage Send(object message, Action<TransportMessageBuilder> builder = null);
        IEnumerable<TransportMessage> Publish(object message, Action<TransportMessageBuilder> builder = null);
        IServiceBus Start();
        void Stop();
    }
}