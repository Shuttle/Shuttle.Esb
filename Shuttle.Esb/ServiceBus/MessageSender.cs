using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class MessageSender : IMessageSender
    {
        private readonly ISubscriptionService _subscriptionService;

        private static readonly IEnumerable<TransportMessage> EmptyPublishFlyweight =
            new ReadOnlyCollection<TransportMessage>(new List<TransportMessage>());

        private readonly IPipelineFactory _pipelineFactory;
        private readonly TransportMessage _transportMessageReceived;

        private readonly ILog _log;

        public MessageSender(IPipelineFactory pipelineFactory, ISubscriptionService subscriptionService)
            : this(pipelineFactory, subscriptionService, null)
        {
        }

        public MessageSender(IPipelineFactory pipelineFactory, ISubscriptionService subscriptionService, TransportMessage transportMessageReceived)
        {
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");
            Guard.AgainstNull(subscriptionService, "subscriptionService");

            _pipelineFactory = pipelineFactory;
            _subscriptionService = subscriptionService;
            _transportMessageReceived = transportMessageReceived;

            _log = Log.For(this);
        }

        public TransportMessage CreateTransportMessage(object message, Action<TransportMessageConfigurator> configure)
        {
            Guard.AgainstNull(message, "message");

            var transportMessagePipeline = _pipelineFactory.GetPipeline<TransportMessagePipeline>();

            try
            {
                var transportMessageConfigurator = new TransportMessageConfigurator(message);

                if (_transportMessageReceived != null)
                {
                    transportMessageConfigurator.TransportMessageReceived(_transportMessageReceived);
                }

                if (configure != null)
                {
                    configure(transportMessageConfigurator);
                }

                if (!transportMessagePipeline.Execute(transportMessageConfigurator))
                {
                    throw new PipelineException(string.Format(EsbResources.PipelineExecutionException,
                        "TransportMessagePipeline", transportMessagePipeline.Exception.AllMessages()));
                }

                return transportMessagePipeline.State.GetTransportMessage();
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(transportMessagePipeline);
            }
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

            var result = CreateTransportMessage(message, configure);

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

            var subscribers = _subscriptionService.GetSubscribedUris(message).ToList();

            if (subscribers.Count > 0)
            {
                var result = new List<TransportMessage>();

                foreach (var subscriber in subscribers)
                {
                    var transportMessage = CreateTransportMessage(message, configure);

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