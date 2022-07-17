using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public interface IMessageSender
    {
        void Dispatch(TransportMessage transportMessage);

        TransportMessage Send(object message);
        TransportMessage Send(object message, Action<TransportMessageBuilder> configure);
        IEnumerable<TransportMessage> Publish(object message);
        IEnumerable<TransportMessage> Publish(object message, Action<TransportMessageBuilder> configure);
    }
}