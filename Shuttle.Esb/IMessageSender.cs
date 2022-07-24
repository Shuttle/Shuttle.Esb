using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public interface IMessageSender
    {
        void Dispatch(TransportMessage transportMessage, TransportMessage transportMessageReceived = null);
        TransportMessage Send(object message, Action<TransportMessageBuilder> builder = null);
        IEnumerable<TransportMessage> Publish(object message, Action<TransportMessageBuilder> builder = null);
    }
}