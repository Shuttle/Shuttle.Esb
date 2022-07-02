using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class MessageSender : IMessageSender
    {
        private readonly ILog _log;
        private readonly HashSet<string> _messageTypesPublishedWarning = new HashSet<string>();

        private readonly IPipelineFactory _pipelineFactory;
        private readonly ISubscriptionManager _subscriptionManager;

        private readonly ITransportMessageFactory _transportMessageFactory;
        private readonly TransportMessage _transportMessageReceived;

        public MessageSender(ITransportMessageFactory transportMessageFactory, IPipelineFactory pipelineFactory,
            ISubscriptionManager subscriptionManager)
            : this(transportMessageFactory, pipelineFactory, subscriptionManager, null)
        {
        }

        public MessageSender(ITransportMessageFactory transportMessageFactory, IPipelineFactory pipelineFactory,
            ISubscriptionManager subscriptionManager, TransportMessage transportMessageReceived)
        {
            Guard.AgainstNull(transportMessageFactory, nameof(transportMessageFactory));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(subscriptionManager, nameof(subscriptionManager));

            _transportMessageFactory = transportMessageFactory;
            _pipelineFactory = pipelineFactory;
            _subscriptionManager = subscriptionManager;
            _transportMessageReceived = transportMessageReceived;

            _log = Log.For(this);
        }

        public void Dispatch(TransportMessage transportMessage)
        {
            Guard.AgainstNull(transportMessage, nameof(transportMessage));

            var messagePipeline = _pipelineFactory.GetPipeline<DispatchTransportMessagePipeline>();

            try
            {
                messagePipeline.Execute(transportMessage, _transportMessageReceived);
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(messagePipeline);
            }
        }

        public TransportMessage Send(object message)
        {
            return Send(message, null);
        }

        public TransportMessage Send(object message, Action<TransportMessageConfigurator> configure)
        {
            Guard.AgainstNull(message, nameof(message));

            var result = _transportMessageFactory.Create(message, configure, _transportMessageReceived);

            Dispatch(result);

            return result;
        }

        public IEnumerable<TransportMessage> Publish(object message)
        {
            return Publish(message, null);
        }

        public IEnumerable<TransportMessage> Publish(object message, Action<TransportMessageConfigurator> configure)
        {
            Guard.AgainstNull(message, nameof(message));

            var subscribers = _subscriptionManager.GetSubscribedUris(message).ToList();

            if (subscribers.Count > 0)
            {
                var result = new List<TransportMessage>(subscribers.Count);

                foreach (var subscriber in subscribers)
                {
                    var transportMessage =
                        _transportMessageFactory.Create(message, configure, _transportMessageReceived);

                    transportMessage.RecipientInboxWorkQueueUri = subscriber;

                    Dispatch(transportMessage);

                    result.Add(transportMessage);
                }

                return result;
            }

            if (!_messageTypesPublishedWarning.Contains(message.GetType().FullName))
            {
                _log.Warning(string.Format(Resources.WarningPublishWithoutSubscribers, message.GetType().FullName));

                _messageTypesPublishedWarning.Add(message.GetType().FullName);
            }

            return Array.Empty<TransportMessage>();
        }
    }
}