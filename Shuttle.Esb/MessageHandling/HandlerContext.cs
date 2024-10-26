using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class HandlerContext<T> : IHandlerContext<T> where T : class
{
    private readonly IMessageSender _messageSender;

    public HandlerContext(IMessageSender messageSender, TransportMessage transportMessage, T message, CancellationToken cancellationToken)
    {
        _messageSender = Guard.AgainstNull(messageSender);
        TransportMessage = Guard.AgainstNull(transportMessage);
        Message = Guard.AgainstNull(message);
        CancellationToken = cancellationToken;
    }

    public TransportMessage TransportMessage { get; }
    public T Message { get; }
    public CancellationToken CancellationToken { get; }
    public ExceptionHandling ExceptionHandling { get; set; } = ExceptionHandling.Default;

    public async Task<TransportMessage> SendAsync(object message, Action<TransportMessageBuilder>? builder = null)
    {
        return await _messageSender.SendAsync(message, TransportMessage, builder).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TransportMessage>> PublishAsync(object message, Action<TransportMessageBuilder>? builder = null)
    {
        return await _messageSender.PublishAsync(message, TransportMessage, builder).ConfigureAwait(false);
    }
}