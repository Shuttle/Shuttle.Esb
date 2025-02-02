using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Esb;

public interface IMessageSender
{
    Task DispatchAsync(TransportMessage transportMessage, TransportMessage? transportMessageReceived = null);
    Task<IEnumerable<TransportMessage>> PublishAsync(object message, TransportMessage? transportMessageReceived = null, Action<TransportMessageBuilder>? builder = null);
    Task<TransportMessage> SendAsync(object message, TransportMessage? transportMessageReceived = null, Action<TransportMessageBuilder>? builder = null);
}