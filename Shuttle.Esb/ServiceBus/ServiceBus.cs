using System;
using System.Collections.Generic;
using Shuttle.Core.Container;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class ServiceBus : IServiceBus
    {
        private readonly ICancellationTokenSource _cancellationTokenSource;
        private readonly IServiceBusConfiguration _configuration;
        private readonly IMessageSender _messageSender;
        private readonly IPipelineFactory _pipelineFactory;

        private IProcessorThreadPool _controlThreadPool;
        private IProcessorThreadPool _deferredMessageThreadPool;
        private IProcessorThreadPool _inboxThreadPool;
        private IProcessorThreadPool _outboxThreadPool;

        public ServiceBus(IServiceBusConfiguration configuration, ITransportMessageFactory transportMessageFactory,
            IPipelineFactory pipelineFactory, ISubscriptionManager subscriptionManager,
            ICancellationTokenSource cancellationTokenSource)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(transportMessageFactory, nameof(transportMessageFactory));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(subscriptionManager, nameof(subscriptionManager));

            _configuration = configuration;
            _pipelineFactory = pipelineFactory;
            _cancellationTokenSource = cancellationTokenSource ?? new DefaultCancellationTokenSource();

            _messageSender = new MessageSender(transportMessageFactory, _pipelineFactory, subscriptionManager);
        }

        public IServiceBus Start()
        {
            if (Started)
            {
                throw new ApplicationException(Resources.ServiceBusInstanceAlreadyStarted);
            }

            ConfigurationInvariant();

            var startupPipeline = _pipelineFactory.GetPipeline<StartupPipeline>();

            Started = true; // required for using ServiceBus in OnStarted event

            try
            {
                startupPipeline.Execute();

                _inboxThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("InboxThreadPool");
                _controlThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("ControlInboxThreadPool");
                _outboxThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("OutboxThreadPool");
                _deferredMessageThreadPool =
                    startupPipeline.State.Get<IProcessorThreadPool>("DeferredMessageThreadPool");
            }
            catch
            {
                Started = false;
                throw;
            }

            return this;
        }

        public void Stop()
        {
            if (!Started)
            {
                return;
            }

            _cancellationTokenSource.Renew();

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

            _pipelineFactory.GetPipeline<ShutdownPipeline>().Execute();

            Started = false;
        }

        public bool Started { get; private set; }

        public void Dispose()
        {
            Stop();

            _cancellationTokenSource.AttemptDispose();
        }

        public void Dispatch(TransportMessage transportMessage)
        {
            StartedGuard();

            _messageSender.Dispatch(transportMessage);
        }

        public TransportMessage Send(object message)
        {
            StartedGuard();

            return _messageSender.Send(message);
        }

        public TransportMessage Send(object message, Action<TransportMessageConfigurator> configure)
        {
            StartedGuard();

            return _messageSender.Send(message, configure);
        }

        public IEnumerable<TransportMessage> Publish(object message)
        {
            StartedGuard();

            return _messageSender.Publish(message);
        }

        public IEnumerable<TransportMessage> Publish(object message, Action<TransportMessageConfigurator> configure)
        {
            StartedGuard();

            return _messageSender.Publish(message, configure);
        }

        private void StartedGuard()
        {
            if (Started)
            {
                return;
            }

            throw new InvalidOperationException(Resources.ServiceBusInstanceNotStarted);
        }

        private void ConfigurationInvariant()
        {
            Guard.Against<WorkerException>(_configuration.IsWorker && !_configuration.HasInbox,
                Resources.WorkerRequiresInboxException);

            if (_configuration.HasInbox)
            {
                Guard.Against<EsbConfigurationException>(
                    _configuration.Inbox.WorkQueue == null && string.IsNullOrEmpty(_configuration.Inbox.WorkQueueUri),
                    string.Format(Resources.RequiredQueueUriMissing, "Inbox.WorkQueueUri"));

                Guard.Against<EsbConfigurationException>(
                    _configuration.Inbox.ErrorQueue == null && string.IsNullOrEmpty(_configuration.Inbox.ErrorQueueUri),
                    string.Format(Resources.RequiredQueueUriMissing, "Inbox.ErrorQueueUri"));
            }

            if (_configuration.HasOutbox)
            {
                Guard.Against<EsbConfigurationException>(
                    _configuration.Outbox.WorkQueue == null && string.IsNullOrEmpty(_configuration.Outbox.WorkQueueUri),
                    string.Format(Resources.RequiredQueueUriMissing, "Outbox.WorkQueueUri"));

                Guard.Against<EsbConfigurationException>(
                    _configuration.Outbox.ErrorQueue == null &&
                    string.IsNullOrEmpty(_configuration.Outbox.ErrorQueueUri),
                    string.Format(Resources.RequiredQueueUriMissing, "Outbox.ErrorQueueUri"));
            }

            if (_configuration.HasControlInbox)
            {
                Guard.Against<EsbConfigurationException>(
                    _configuration.ControlInbox.WorkQueue == null &&
                    string.IsNullOrEmpty(_configuration.ControlInbox.WorkQueueUri),
                    string.Format(Resources.RequiredQueueUriMissing, "ControlInbox.WorkQueueUri"));

                Guard.Against<EsbConfigurationException>(
                    _configuration.ControlInbox.ErrorQueue == null &&
                    string.IsNullOrEmpty(_configuration.ControlInbox.ErrorQueueUri),
                    string.Format(Resources.RequiredQueueUriMissing, "ControlInbox.ErrorQueueUri"));
            }
        }

        [Obsolete("This method has been replaced by `ComponentRegistryExtensions.RegisterServiceBus()`.", false)]
        public static IServiceBusConfiguration Register(IComponentRegistry registry)
        {
            return registry.RegisterServiceBus();
        }

        [Obsolete("Please create an instance of the service bus using `IComponentResolver.Resolve<IServiceBus>()`.")]
        public static IServiceBus Create(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, nameof(resolver));

            return resolver.Resolve<IServiceBus>();
        }
    }
}