using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public interface IMessageSender
    {
        Task Dispatch(TransportMessage transportMessage, TransportMessage transportMessageReceived);
        Task<TransportMessage> Send(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder);
        Task<IEnumerable<TransportMessage>> Publish(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder);
    }
}