using System;
using System.Collections.Generic;
using System.Threading;
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

        public IServiceBus Start()
        {
            return StartAsync(true).GetAwaiter().GetResult();
        }

        public async Task<IServiceBus> StartAsync()
        {
            return await StartAsync(false).ConfigureAwait(false);
        }

        private async Task<IServiceBus> StartAsync(bool sync)
        {
            if (Started)
            {
                throw new ApplicationException(Resources.ServiceBusInstanceAlreadyStarted);
            }

            ConfigurationInvariant();

            var startupPipeline = _pipelineFactory.GetPipeline<StartupPipeline>();

            Started = true; // required for using ServiceBus in OnStarted event
            RunningAsync = !sync;

            try
            {
                if (sync)
                {
                    startupPipeline.Execute(CancellationToken.None);
                }
                else
                {
                    await startupPipeline.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
                }

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

        public void Stop()
        {
            if (RunningAsync)
            {
                throw new InvalidOperationException(string.Format(Resources.IncorrectStopCalledException, "asynchronous", "StopAsync()"));
            }

            StopAsync(true).GetAwaiter().GetResult();
        }

        public async Task StopAsync()
        {
            if (!RunningAsync)
            {
                throw new InvalidOperationException(string.Format(Resources.IncorrectStopCalledException, "synchronous", "Stop()"));
            }

            await StopAsync(false).ConfigureAwait(false);
        }

        private async Task StopAsync(bool sync)
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

            if (sync)
            {
                 _pipelineFactory.GetPipeline<ShutdownPipeline>().Execute(CancellationToken.None);
            }
            else
            {
                await _pipelineFactory.GetPipeline<ShutdownPipeline>().ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
            }

            _pipelineFactory.Flush();

            Started = false;
        }

        public bool Started { get; private set; }
        public bool RunningAsync { get; private set; }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Stop();

            _cancellationTokenSource.TryDispose();

            _disposed = true;
        }

        public TransportMessage Send(object message, Action<TransportMessageBuilder> builder = null)
        {
            StartedGuard();

            return _messageSender.Send(message, null, builder);
        }

        public async Task<TransportMessage> SendAsync(object message, Action<TransportMessageBuilder> builder = null)
        {
            StartedGuard();

            return await _messageSender.SendAsync(message, null, builder).ConfigureAwait(false);
        }

        public IEnumerable<TransportMessage> Publish(object message, Action<TransportMessageBuilder> builder = null)
        {
            StartedGuard();

            return _messageSender.Publish(message, null, builder);
        }

        public Task<IEnumerable<TransportMessage>> PublishAsync(object message, Action<TransportMessageBuilder> builder = null)
        {
            StartedGuard();

            return _messageSender.PublishAsync(message, null, builder);
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

            await StopAsync().ConfigureAwait(false);

            _cancellationTokenSource.TryDispose();

            _disposed = true;
        }
    }
}