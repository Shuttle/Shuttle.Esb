using System;

namespace Shuttle.Esb
{
    public interface ITransportMessageFactory
    {
        TransportMessage Create(object message, Action<TransportMessageConfigurator> configure);
        TransportMessage Create(object message, TransportMessage transportMessageReceived);

        TransportMessage Create(object message, Action<TransportMessageConfigurator> configure,
            TransportMessage transportMessageReceived);
    }
}