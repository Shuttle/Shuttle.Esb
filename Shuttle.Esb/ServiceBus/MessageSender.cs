using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class MessageSender : IMessageSender
    {
        private readonly ISubscriptionManager _subscriptionManager;

        private static readonly IEnumerable<TransportMessage> EmptyPublishFlyweight =
            new ReadOnlyCollection<TransportMessage>(new List<TransportMessage>());

        private readonly ITransportMessageFactory _transportMessageFactory;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly TransportMessage _transportMessageReceived;

        private readonly ILog _log;

        public MessageSender(ITransportMessageFactory transportMessageFactory, IPipelineFactory pipelineFactory, ISubscriptionManager subscriptionManager)
            : this(transportMessageFactory, pipelineFactory, subscriptionManager, null)
        {
        }

        public MessageSender(ITransportMessageFactory transportMessageFactory, IPipelineFactory pipelineFactory, ISubscriptionManager subscriptionManager, TransportMessage transportMessageReceived)
        {
            Guard.AgainstNull(transportMessageFactory, "transportMessageFactory");
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");
            Guard.AgainstNull(subscriptionManager, "subscriptionManager");

            _transportMessageFactory = transportMessageFactory;
            _pipelineFactory = pipelineFactory;
            _subscriptionManager = subscriptionManager;
            _transportMessageReceived = transportMessageReceived;

            _log = Log.For(this);
        }

        public void Dispatch(TransportMessage transportMessage)
        {
            Guard.AgainstNull(transportMessage, "transportMessage");

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
            Guard.AgainstNull(message, "message");

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
            Guard.AgainstNull(message, "message");

            var subscribers = _subscriptionManager.GetSubscribedUris(message).ToList();

            if (subscribers.Count > 0)
            {
                var result = new List<TransportMessage>();

                foreach (var subscriber in subscribers)
                {
                    var transportMessage = _transportMessageFactory.Create(message, configure, _transportMessageReceived);

                    transportMessage.RecipientInboxWorkQueueUri = subscriber;

                    Dispatch(transportMessage);

                    result.Add(transportMessage);
                }

                return result;
            }

            _log.Warning(string.Format(EsbResources.WarningPublishWithoutSubscribers, message.GetType().FullName));

            return EmptyPublishFlyweight;
        }
    }
}