using System;
using System.Collections.Generic;
using System.Reflection;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class ServiceBus : IServiceBus
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly ISubscriptionService _subscriptionService;
        private IMessageSender _messageSender;

        private IProcessorThreadPool _controlThreadPool;
        private IProcessorThreadPool _inboxThreadPool;
        private IProcessorThreadPool _outboxThreadPool;
        private IProcessorThreadPool _deferredMessageThreadPool;
        private readonly IServiceBusConfiguration _configuration;

        public ServiceBus(IServiceBusConfiguration configuration, IPipelineFactory pipelineFactory, ISubscriptionService subscriptionService)
        {
            Guard.AgainstNull(configuration, "configuration");
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");
            Guard.AgainstNull(subscriptionService, "subscriptionService");

            _pipelineFactory = pipelineFactory;
            _subscriptionService = subscriptionService;

            _configuration = configuration;
        }

        public IServiceBus Start()
        {
            if (Started)
            {
                throw new ApplicationException(EsbResources.ServiceBusInstanceAlreadyStarted);
            }

            _configuration.Invariant();

            var startupPipeline = _pipelineFactory.GetPipeline<StartupPipeline>();

            startupPipeline.Execute();

            _inboxThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("InboxThreadPool");
            _controlThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("ControlInboxThreadPool");
            _outboxThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("OutboxThreadPool");
            _deferredMessageThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("DeferredMessageThreadPool");

            _messageSender = new MessageSender(_pipelineFactory, _subscriptionService);

            Started = true;

            return this;
        }

        public void Stop()
        {
            if (!Started)
            {
                return;
            }

            if (_configuration.HasInbox)
            {
                if (_configuration.Inbox.HasDeferredQueue)
                {
                    _deferredMessageThreadPool.Dispose();
                }

                _inboxThreadPool.Dispose();
            }

            if (_configuration.HasControlInbox)
            {
                _controlThreadPool.Dispose();
            }

            if (_configuration.HasOutbox)
            {
                _outboxThreadPool.Dispose();
            }

            Started = false;
        }

        public bool Started { get; private set; }

        public void Dispose()
        {
            Stop();
        }

        public static IServiceBus Create()
        {
            return Create(null);
        }

        public static IServiceBus Create(Action<DefaultConfigurator> configure)
        {
            var configurator = new DefaultConfigurator();

            if (configure != null)
            {
                configure.Invoke(configurator);
            }

            return new ServiceBus(configurator.Configuration(), configurator.Container.Resolve<IPipelineFactory>(), configurator.Container.Resolve<ISubscriptionService>());
        }

        public TransportMessage CreateTransportMessage(object message, Action<TransportMessageConfigurator> configure)
        {
            return _messageSender.CreateTransportMessage(message, configure);
        }

        public void Dispatch(TransportMessage transportMessage)
        {
            _messageSender.Dispatch(transportMessage);
        }

        public TransportMessage Send(object message)
        {
            return _messageSender.Send(message);
        }

        public TransportMessage Send(object message, Action<TransportMessageConfigurator> configure)
        {
            return _messageSender.Send(message, configure);
        }

        public IEnumerable<TransportMessage> Publish(object message)
        {
            return _messageSender.Publish(message);
        }

        public IEnumerable<TransportMessage> Publish(object message, Action<TransportMessageConfigurator> configure)
        {
            return _messageSender.Publish(message, configure);
        }
    }
}