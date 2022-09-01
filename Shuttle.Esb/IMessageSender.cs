using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public interface IMessageSender
    {
        void Dispatch(TransportMessage transportMessage, TransportMessage transportMessageReceived);
        TransportMessage Send(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder);
        IEnumerable<TransportMessage> Publish(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder);
    }
}