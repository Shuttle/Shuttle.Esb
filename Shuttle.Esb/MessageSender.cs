using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class MessageSender : IMessageSender
    {
        private readonly HashSet<string> _messageTypesPublishedWarning = new HashSet<string>();

        private readonly IPipelineFactory _pipelineFactory;
        private readonly ISubscriptionService _subscriptionService;

        public MessageSender(IPipelineFactory pipelineFactory, ISubscriptionService subscriptionService)
        {
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(subscriptionService, nameof(subscriptionService));

            _pipelineFactory = pipelineFactory;
            _subscriptionService = subscriptionService;
        }

        public void Dispatch(TransportMessage transportMessage, TransportMessage transportMessageReceived = null)
        {
            Guard.AgainstNull(transportMessage, nameof(transportMessage));

            var messagePipeline = _pipelineFactory.GetPipeline<DispatchTransportMessagePipeline>();

            try
            {
                messagePipeline.Execute(transportMessage, transportMessageReceived);
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(messagePipeline);
            }
        }

        public TransportMessage Send(object message, Action<TransportMessageBuilder> builder = null)
        {
            var transportMessage = GetTransportMessage(message, builder);

            Dispatch(transportMessage);

            return transportMessage;
        }

        private TransportMessage GetTransportMessage(object message, Action<TransportMessageBuilder> builder)
        {
            Guard.AgainstNull(message, nameof(message));

            var messagePipeline = _pipelineFactory.GetPipeline<TransportMessagePipeline>();

            try
            {
                messagePipeline.Execute(message, builder);

                return messagePipeline.State.GetTransportMessage();
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(messagePipeline);
            }
        }

        public IEnumerable<TransportMessage> Publish(object message, Action<TransportMessageBuilder> builder = null)
        {
            Guard.AgainstNull(message, nameof(message));

            var subscribers = _subscriptionService.GetSubscribedUris(message).ToList();

            if (subscribers.Count > 0)
            {
                var transportMessage = GetTransportMessage(message, builder);

                var result = new List<TransportMessage>(subscribers.Count);

                foreach (var subscriber in subscribers)
                {
                    transportMessage.RecipientInboxWorkQueueUri = subscriber;

                    Dispatch(transportMessage);
                    
                    result.Add(transportMessage);
                }

                return result;
            }

            if (!_messageTypesPublishedWarning.Contains(message.GetType().FullName))
            {
                _messageTypesPublishedWarning.Add(message.GetType().FullName);
            }

            return Enumerable.Empty<TransportMessage>();
        }
    }
}