using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task Dispatch(TransportMessage transportMessage, TransportMessage transportMessageReceived)
        {
            Guard.AgainstNull(transportMessage, nameof(transportMessage));

            var messagePipeline = _pipelineFactory.GetPipeline<DispatchTransportMessagePipeline>();

            try
            {
                await messagePipeline.Execute(transportMessage, transportMessageReceived).ConfigureAwait(false);
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(messagePipeline);
            }
        }

        public async Task<TransportMessage> Send(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder)
        {
            var transportMessage = await GetTransportMessage(message, transportMessageReceived, builder).ConfigureAwait(false);

            await Dispatch(transportMessage, transportMessageReceived);

            return transportMessage;
        }

        private async Task<TransportMessage> GetTransportMessage(object message, TransportMessage transportMessageReceived,
            Action<TransportMessageBuilder> builder)
        {
            Guard.AgainstNull(message, nameof(message));

            var messagePipeline = _pipelineFactory.GetPipeline<TransportMessagePipeline>();

            try
            {
                await messagePipeline.Execute(message, transportMessageReceived, builder).ConfigureAwait(false);

                return messagePipeline.State.GetTransportMessage();
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(messagePipeline);
            }
        }

        public async Task<IEnumerable<TransportMessage>> Publish(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder)
        {
            Guard.AgainstNull(message, nameof(message));

            var subscribers = (await _subscriptionService.GetSubscribedUris(message).ConfigureAwait(false)).ToList();

            if (subscribers.Count > 0)
            {
                var transportMessage = await GetTransportMessage(message, transportMessageReceived, builder).ConfigureAwait(false);

                var result = new List<TransportMessage>(subscribers.Count);

                foreach (var subscriber in subscribers)
                {
                    transportMessage.RecipientInboxWorkQueueUri = subscriber;

                    await Dispatch(transportMessage, transportMessageReceived).ConfigureAwait(false);
                    
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