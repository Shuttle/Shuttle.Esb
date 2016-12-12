using System;
using System.Collections.Generic;
using System.Reflection;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class ServiceBus : IServiceBus
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly ISubscriptionManager _subscriptionManager;
        private IMessageSender _messageSender;

        private IProcessorThreadPool _controlThreadPool;
        private IProcessorThreadPool _inboxThreadPool;
        private IProcessorThreadPool _outboxThreadPool;
        private IProcessorThreadPool _deferredMessageThreadPool;
        private readonly IServiceBusConfiguration _configuration;

        public ServiceBus(IServiceBusConfiguration configuration, ITransportMessageFactory transportMessageFactory, IPipelineFactory pipelineFactory, ISubscriptionManager subscriptionManager)
        {
            Guard.AgainstNull(configuration, "configuration");
            Guard.AgainstNull(transportMessageFactory, "transportMessageFactory");
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");
            Guard.AgainstNull(subscriptionManager, "subscriptionManager");

            _configuration = configuration;
            _pipelineFactory = pipelineFactory;
            _subscriptionManager = subscriptionManager;

            _messageSender = new MessageSender(transportMessageFactory, _pipelineFactory, _subscriptionManager);
        }

        public IServiceBus Start()
        {
            if (Started)
            {
                throw new ApplicationException(EsbResources.ServiceBusInstanceAlreadyStarted);
            }

            ConfigurationInvariant();

            var startupPipeline = _pipelineFactory.GetPipeline<StartupPipeline>();

            startupPipeline.Execute();

            _inboxThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("InboxThreadPool");
            _controlThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("ControlInboxThreadPool");
            _outboxThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("OutboxThreadPool");
            _deferredMessageThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("DeferredMessageThreadPool");

            Started = true;

            return this;
        }

        private void ConfigurationInvariant()
        {
            Guard.Against<WorkerException>(_configuration.IsWorker && !_configuration.HasInbox,
                EsbResources.WorkerRequiresInboxException);

            if (_configuration.HasInbox)
            {
                Guard.Against<EsbConfigurationException>(_configuration.Inbox.WorkQueue == null && string.IsNullOrEmpty(_configuration.Inbox.WorkQueueUri),
                    string.Format(EsbResources.RequiredQueueUriMissing, "Inbox.WorkQueueUri"));

                Guard.Against<EsbConfigurationException>(_configuration.Inbox.ErrorQueue == null && string.IsNullOrEmpty(_configuration.Inbox.ErrorQueueUri),
                    string.Format(EsbResources.RequiredQueueUriMissing, "Inbox.ErrorQueueUri"));
            }

            if (_configuration.HasOutbox)
            {
                Guard.Against<EsbConfigurationException>(_configuration.Outbox.WorkQueue == null && string.IsNullOrEmpty(_configuration.Outbox.WorkQueueUri),
                    string.Format(EsbResources.RequiredQueueUriMissing, "Outbox.WorkQueueUri"));

                Guard.Against<EsbConfigurationException>(_configuration.Outbox.ErrorQueue == null && string.IsNullOrEmpty(_configuration.Outbox.ErrorQueueUri),
                    string.Format(EsbResources.RequiredQueueUriMissing, "Outbox.ErrorQueueUri"));
            }

            if (_configuration.HasControlInbox)
            {
                Guard.Against<EsbConfigurationException>(_configuration.ControlInbox.WorkQueue == null && string.IsNullOrEmpty(_configuration.ControlInbox.WorkQueueUri),
                    string.Format(EsbResources.RequiredQueueUriMissing, "ControlInbox.WorkQueueUri"));

                Guard.Against<EsbConfigurationException>(_configuration.ControlInbox.ErrorQueue == null && string.IsNullOrEmpty(_configuration.ControlInbox.ErrorQueueUri),
                    string.Format(EsbResources.RequiredQueueUriMissing, "ControlInbox.ErrorQueueUri"));
            }
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

        public static IServiceBus Create(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, "resolver");

            var configuration = resolver.Resolve<IServiceBusConfiguration>();

            if (configuration == null)
            {
                throw new InvalidOperationException(string.Format(InfrastructureResources.TypeNotRegisteredException, typeof(IServiceBusConfiguration).FullName));
            }

            configuration.Assign(resolver);

            var defaultPipelineFactory = resolver.Resolve<IPipelineFactory>() as DefaultPipelineFactory;

            if (defaultPipelineFactory  != null)
            {
                defaultPipelineFactory.Assign(resolver);
            }

            return resolver.Resolve<IServiceBus>();
        }
    }
}