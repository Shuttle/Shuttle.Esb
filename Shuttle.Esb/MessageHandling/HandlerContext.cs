using System;
using System.Collections.Generic;
using System.Threading;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class HandlerContext<T> : IHandlerContext<T> where T : class
    {
        private readonly IMessageSender _messageSender;

        public HandlerContext(ITransportMessageFactory transportMessageFactory,
            IPipelineFactory pipelineFactory, ISubscriptionService subscriptionService,
            TransportMessage transportMessage, T message, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(transportMessage, nameof(transportMessage));
            Guard.AgainstNull(message, nameof(message));

            _messageSender = new MessageSender(transportMessageFactory, pipelineFactory, subscriptionService,
                transportMessage);

            TransportMessage = transportMessage;
            Message = message;
            CancellationToken = cancellationToken;
        }

        public TransportMessage TransportMessage { get; }
        public T Message { get; }
        public CancellationToken CancellationToken { get; }
        public ExceptionHandling ExceptionHandler { get; } = new ExceptionHandling();

        public void Dispatch(TransportMessage transportMessage)
        {
            _messageSender.Dispatch(transportMessage);
        }

        public TransportMessage Send(object message)
        {
            return _messageSender.Send(message);
        }

        public TransportMessage Send(object message, Action<TransportMessageBuilder> configure)
        {
            return _messageSender.Send(message, configure);
        }

        public IEnumerable<TransportMessage> Publish(object message)
        {
            return _messageSender.Publish(message);
        }

        public IEnumerable<TransportMessage> Publish(object message, Action<TransportMessageBuilder> configure)
        {
            return _messageSender.Publish(message, configure);
        }
    }
}