using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public interface IMessageSender
    {
        void Dispatch(TransportMessage transportMessage, TransportMessage transportMessageReceived);
        Task DispatchAsync(TransportMessage transportMessage, TransportMessage transportMessageReceived);
        TransportMessage Send(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder);
        Task<TransportMessage> SendAsync(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder);
        IEnumerable<TransportMessage> Publish(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder);
        Task<IEnumerable<TransportMessage>> PublishAsync(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder);
    }
}