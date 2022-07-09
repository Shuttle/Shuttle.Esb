using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
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
        private readonly ServiceBusOptions _options;

        public ServiceBus(IOptions<ServiceBusOptions> options, IServiceBusConfiguration configuration, ITransportMessageFactory transportMessageFactory,
            IPipelineFactory pipelineFactory, ISubscriptionManager subscriptionManager,
            ICancellationTokenSource cancellationTokenSource)
        {
            Guard.AgainstNull(options, nameof(options));
            Guard.AgainstNull(options.Value, nameof(options.Value));
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(transportMessageFactory, nameof(transportMessageFactory));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(subscriptionManager, nameof(subscriptionManager));

            _options = options.Value;
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

            if (_configuration.HasInbox())
            {
                if (_configuration.Inbox.HasDeferredQueue())
                {
                    _deferredMessageThreadPool.Dispose();
                }

                _inboxThreadPool.Dispose();
            }

            if (_configuration.HasControlInbox())
            {
                _controlThreadPool.Dispose();
            }

            if (_configuration.HasOutbox())
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
            Guard.Against<WorkerException>(_configuration.IsWorker && !_configuration.HasInbox(),
                Resources.WorkerRequiresInboxException);

            if (_configuration.HasInbox())
            {
                Guard.Against<EsbConfigurationException>(
                    _configuration.Inbox.WorkQueue == null && string.IsNullOrEmpty(_options.Inbox.WorkQueueUri),
                    string.Format(Resources.RequiredQueueUriMissingException, "Inbox.WorkQueueUri"));

                Guard.Against<EsbConfigurationException>(
                    _options.Inbox == null,
                    string.Format(Resources.RequiredOptionsMissingException, "Inbox"));
            }

            if (_configuration.HasOutbox())
            {
                Guard.Against<EsbConfigurationException>(
                    _configuration.Outbox.WorkQueue == null && string.IsNullOrEmpty(_options.Outbox.WorkQueueUri),
                    string.Format(Resources.RequiredQueueUriMissingException, "Outbox.WorkQueueUri"));

                Guard.Against<EsbConfigurationException>(
                    _options.Outbox == null,
                    string.Format(Resources.RequiredOptionsMissingException, "Outbox"));
            }

            if (_configuration.HasControlInbox())
            {
                Guard.Against<EsbConfigurationException>(
                    _configuration.ControlInbox.WorkQueue == null &&
                    string.IsNullOrEmpty(_options.ControlInbox.WorkQueueUri),
                    string.Format(Resources.RequiredQueueUriMissingException, "ControlInbox.WorkQueueUri"));

                Guard.Against<EsbConfigurationException>(
                    _options.ControlInbox == null,
                    string.Format(Resources.RequiredOptionsMissingException, "ControlInbox"));
            }
        }
    }
}