using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class HandlerContext<T> : IHandlerContext<T> where T : class
    {
        private readonly IMessageSender _messageSender;

        public HandlerContext(IMessageSender messageSender, TransportMessage transportMessage, T message, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(messageSender, nameof(messageSender));
            Guard.AgainstNull(transportMessage, nameof(transportMessage));
            Guard.AgainstNull(message, nameof(message));

            _messageSender = messageSender;

            TransportMessage = transportMessage;
            Message = message;
            CancellationToken = cancellationToken;
        }

        public TransportMessage TransportMessage { get; }
        public T Message { get; }
        public CancellationToken CancellationToken { get; }
        public ExceptionHandling ExceptionHandling { get; } = new ExceptionHandling();

        public async Task<TransportMessage> Send(object message, Action<TransportMessageBuilder> builder = null)
        {
            return await _messageSender.Send(message, TransportMessage, builder).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TransportMessage>> Publish(object message, Action<TransportMessageBuilder> builder = null)
        {
            return await _messageSender.Publish(message, TransportMessage, builder).ConfigureAwait(false);
        }
    }
}