using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly IServiceBusConfiguration _serviceBusConfiguration;
        private readonly IMessageSender _messageSender;
        private readonly IPipelineFactory _pipelineFactory;

        private IProcessorThreadPool _controlInboxThreadPool;
        private IProcessorThreadPool _deferredMessageThreadPool;
        private IProcessorThreadPool _inboxThreadPool;
        private IProcessorThreadPool _outboxThreadPool;

        private readonly ServiceBusOptions _serviceBusOptions;

        private bool _disposed;

        public ServiceBus(IOptions<ServiceBusOptions> serviceBusOptions,
            IServiceBusConfiguration serviceBusConfiguration,
            IPipelineFactory pipelineFactory, IMessageSender messageSender,
            ICancellationTokenSource cancellationTokenSource)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(messageSender, nameof(messageSender));

            _serviceBusOptions = serviceBusOptions.Value;
            _serviceBusConfiguration = serviceBusConfiguration;
            _pipelineFactory = pipelineFactory;
            _cancellationTokenSource = cancellationTokenSource ?? new DefaultCancellationTokenSource();
            _messageSender = messageSender;
        }

        public async Task<IServiceBus> Start()
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
                await startupPipeline.Execute().ConfigureAwait(false);

                _inboxThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("InboxThreadPool");
                _controlInboxThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("ControlInboxThreadPool");
                _outboxThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("OutboxThreadPool");
                _deferredMessageThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("DeferredMessageThreadPool");
            }
            catch
            {
                Started = false;
                throw;
            }

            return this;
        }

        public async Task Stop()
        {
            if (!Started)
            {
                return;
            }

            _cancellationTokenSource.Renew();

            if (_serviceBusConfiguration.HasInbox())
            {
                if (_serviceBusConfiguration.Inbox.HasDeferredQueue())
                {
                    _deferredMessageThreadPool.Dispose();
                }

                _inboxThreadPool.Dispose();
            }

            if (_serviceBusConfiguration.HasControlInbox())
            {
                _controlInboxThreadPool.Dispose();
            }

            if (_serviceBusConfiguration.HasOutbox())
            {
                _outboxThreadPool.Dispose();
            }

            await _pipelineFactory.GetPipeline<ShutdownPipeline>().Execute().ConfigureAwait(false);

            _pipelineFactory.Flush();

            Started = false;
        }

        public bool Started { get; private set; }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Stop().Wait();

            _cancellationTokenSource.AttemptDispose();

            _disposed = true;
        }

        public async Task<TransportMessage> Send(object message, Action<TransportMessageBuilder> builder = null)
        {
            StartedGuard();

            return await _messageSender.Send(message, null, builder).ConfigureAwait(false);
        }

        public Task<IEnumerable<TransportMessage>> Publish(object message, Action<TransportMessageBuilder> builder = null)
        {
            StartedGuard();

            return _messageSender.Publish(message, null, builder);
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
            Guard.Against<WorkerException>(_serviceBusConfiguration.IsWorker() && !_serviceBusConfiguration.HasInbox(),
                Resources.WorkerRequiresInboxException);

            if (_serviceBusConfiguration.HasInbox())
            {
                Guard.Against<InvalidOperationException>(
                    _serviceBusConfiguration.Inbox.WorkQueue == null && string.IsNullOrEmpty(_serviceBusOptions.Inbox.WorkQueueUri),
                    string.Format(Resources.RequiredQueueUriMissingException, "Inbox.WorkQueueUri"));

                Guard.Against<InvalidOperationException>(
                    _serviceBusOptions.Inbox == null,
                    string.Format(Resources.RequiredOptionsMissingException, "Inbox"));
            }

            if (_serviceBusConfiguration.HasOutbox())
            {
                Guard.Against<InvalidOperationException>(
                    _serviceBusConfiguration.Outbox.WorkQueue == null && string.IsNullOrEmpty(_serviceBusOptions.Outbox.WorkQueueUri),
                    string.Format(Resources.RequiredQueueUriMissingException, "Outbox.WorkQueueUri"));

                Guard.Against<InvalidOperationException>(
                    _serviceBusOptions.Outbox == null,
                    string.Format(Resources.RequiredOptionsMissingException, "Outbox"));
            }

            if (_serviceBusConfiguration.HasControlInbox())
            {
                Guard.Against<InvalidOperationException>(
                    _serviceBusConfiguration.ControlInbox.WorkQueue == null &&
                    string.IsNullOrEmpty(_serviceBusOptions.ControlInbox.WorkQueueUri),
                    string.Format(Resources.RequiredQueueUriMissingException, "ControlInbox.WorkQueueUri"));

                Guard.Against<InvalidOperationException>(
                    _serviceBusOptions.ControlInbox == null,
                    string.Format(Resources.RequiredOptionsMissingException, "ControlInbox"));
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            await Stop().ConfigureAwait(false);

            _cancellationTokenSource.AttemptDispose();

            _disposed = true;
        }
    }
}