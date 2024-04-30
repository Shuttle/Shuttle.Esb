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

        public void Dispatch(TransportMessage transportMessage, TransportMessage transportMessageReceived)
        {
            DispatchAsync(transportMessage, transportMessageReceived, true).GetAwaiter().GetResult();
        }

        public async Task DispatchAsync(TransportMessage transportMessage, TransportMessage transportMessageReceived)
        {
            await DispatchAsync(transportMessage, transportMessageReceived, false).ConfigureAwait(false);
        }

        private async Task DispatchAsync(TransportMessage transportMessage, TransportMessage transportMessageReceived, bool sync)
        {
            Guard.AgainstNull(transportMessage, nameof(transportMessage));

            var messagePipeline = _pipelineFactory.GetPipeline<DispatchTransportMessagePipeline>();

            try
            {
                if (sync)
                {
                    messagePipeline.Execute(transportMessage, transportMessageReceived);
                }
                else
                {
                    await messagePipeline.ExecuteAsync(transportMessage, transportMessageReceived).ConfigureAwait(false);
                }
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(messagePipeline);
            }
        }

        public TransportMessage Send(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder)
        {
            return SendAsync(message, transportMessageReceived, builder, true).GetAwaiter().GetResult();
        }

        public async Task<TransportMessage> SendAsync(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder)
        {
            return await SendAsync(message, transportMessageReceived, builder, false).ConfigureAwait(false);
        }

        private async Task<TransportMessage> SendAsync(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder, bool sync)
        {
            var transportMessage = sync
                ? GetTransportMessageAsync(message, transportMessageReceived, builder, true).GetAwaiter().GetResult()
                : await GetTransportMessageAsync(message, transportMessageReceived, builder, false).ConfigureAwait(false);

            if (sync)
            {
                DispatchAsync(transportMessage, transportMessageReceived, true ).GetAwaiter().GetResult();
            }
            else
            {
                await DispatchAsync(transportMessage, transportMessageReceived, false).ConfigureAwait(false);
            }

            return transportMessage;
        }

        public IEnumerable<TransportMessage> Publish(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder)
        {
            return PublishAsync(message, transportMessageReceived, builder, true).GetAwaiter().GetResult();
        }

        private async Task<TransportMessage> GetTransportMessageAsync(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder, bool sync)
        {
            Guard.AgainstNull(message, nameof(message));

            var messagePipeline = _pipelineFactory.GetPipeline<TransportMessagePipeline>();

            try
            {
                if (sync)
                {
                    messagePipeline.Execute(message, transportMessageReceived, builder);
                }
                else
                {
                    await messagePipeline.ExecuteAsync(message, transportMessageReceived, builder).ConfigureAwait(false);
                }

                return messagePipeline.State.GetTransportMessage();
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(messagePipeline);
            }
        }

        public async Task<IEnumerable<TransportMessage>> PublishAsync(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder)
        {
            return await PublishAsync(message, transportMessageReceived, builder, false).ConfigureAwait(false);
        }

        private async Task<IEnumerable<TransportMessage>> PublishAsync(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder, bool sync)
        {
            Guard.AgainstNull(message, nameof(message));

            var subscribers = (sync 
                ? _subscriptionService.GetSubscribedUris(message)
                : await _subscriptionService.GetSubscribedUrisAsync(message).ConfigureAwait(false)).ToList();

            if (subscribers.Count > 0)
            {
                var transportMessage = sync
                ? GetTransportMessageAsync(message, transportMessageReceived, builder, true).GetAwaiter().GetResult()
                : await GetTransportMessageAsync(message, transportMessageReceived, builder, false).ConfigureAwait(false);

                var result = new List<TransportMessage>(subscribers.Count);

                foreach (var subscriber in subscribers)
                {
                    transportMessage.RecipientInboxWorkQueueUri = subscriber;

                    if (sync)
                    {
                        Dispatch(transportMessage, transportMessageReceived);
                    }
                    else
                    {
                        await DispatchAsync(transportMessage, transportMessageReceived).ConfigureAwait(false);
                    }

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