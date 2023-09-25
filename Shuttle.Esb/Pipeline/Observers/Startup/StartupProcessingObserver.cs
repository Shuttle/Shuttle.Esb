using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public interface IStartupProcessingObserver : 
        IPipelineObserver<OnCreatePhysicalQueues>, 
        IPipelineObserver<OnStartInboxProcessing>, 
        IPipelineObserver<OnStartControlInboxProcessing>, 
        IPipelineObserver<OnStartOutboxProcessing>, 
        IPipelineObserver<OnStartDeferredMessageProcessing>
    {
    }

    public class StartupProcessingObserver : IStartupProcessingObserver
    {
        private readonly IServiceBus _serviceBus;
        private readonly ServiceBusOptions _serviceBusOptions;
        private readonly IServiceBusConfiguration _serviceBusConfiguration;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IPipelineThreadActivity _pipelineThreadActivity;
        private readonly IWorkerAvailabilityService _workerAvailabilityService;

        public StartupProcessingObserver(IOptions<ServiceBusOptions> serviceBusOptions, IServiceBus serviceBus,
            IServiceBusConfiguration serviceBusConfiguration, IWorkerAvailabilityService workerAvailabilityService,
            IPipelineFactory pipelineFactory, IPipelineThreadActivity pipelineThreadActivity)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));
            Guard.AgainstNull(serviceBus, nameof(serviceBus));
            Guard.AgainstNull(serviceBusConfiguration,  nameof(serviceBusConfiguration));
            Guard.AgainstNull(workerAvailabilityService, nameof(workerAvailabilityService));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(pipelineThreadActivity, nameof(pipelineThreadActivity));

            _serviceBus = serviceBus;
            _serviceBusOptions = serviceBusOptions.Value;
            _workerAvailabilityService = workerAvailabilityService;
            _pipelineFactory = pipelineFactory;
            _pipelineThreadActivity = pipelineThreadActivity;
            _serviceBusConfiguration = serviceBusConfiguration;
        }

        public void Execute(OnCreatePhysicalQueues pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnCreatePhysicalQueues pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnCreatePhysicalQueues pipelineEvent, bool sync)
        {
            if (!_serviceBusOptions.CreatePhysicalQueues)
            {
                return;
            }

            if (sync)
            {
                _serviceBusConfiguration.CreatePhysicalQueues();
            }
            else
            {
                await _serviceBusConfiguration.CreatePhysicalQueuesAsync().ConfigureAwait(false);
            }
        }

        public void Execute(OnStartControlInboxProcessing pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnStartControlInboxProcessing pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnStartControlInboxProcessing pipelineEvent, bool sync)
        {
            if (!_serviceBusConfiguration.HasControlInbox())
            {
                return;
            }

            var processorThreadPool = new ProcessorThreadPool(
                "ControlInboxProcessor",
                _serviceBusOptions.ControlInbox.ThreadCount,
                new ControlInboxProcessorFactory(_serviceBusOptions, _pipelineFactory, _pipelineThreadActivity),
                _serviceBusOptions.ProcessorThread);

            pipelineEvent.Pipeline.State.Add("ControlInboxThreadPool", sync ? processorThreadPool.Start() : await processorThreadPool.StartAsync());

            await Task.CompletedTask.ConfigureAwait(false);
        }

        public void Execute(OnStartDeferredMessageProcessing pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnStartDeferredMessageProcessing pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnStartDeferredMessageProcessing pipelineEvent, bool sync)
        {
            if (!_serviceBusConfiguration.HasInbox() || !_serviceBusConfiguration.Inbox.HasDeferredQueue())
            {
                return;
            }

            _serviceBusConfiguration.Inbox.DeferredMessageProcessor = new DeferredMessageProcessor(_serviceBusOptions, _pipelineFactory);

            var processorThreadPool = new ProcessorThreadPool(
                "DeferredMessageProcessor",
                1,
                new DeferredMessageProcessorFactory(_serviceBusConfiguration),
                _serviceBusOptions.ProcessorThread);

            pipelineEvent.Pipeline.State.Add("DeferredMessageThreadPool", sync ? processorThreadPool.Start() : await processorThreadPool.StartAsync());

            await Task.CompletedTask.ConfigureAwait(false);
        }

        public void Execute(OnStartInboxProcessing pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnStartInboxProcessing pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnStartInboxProcessing pipelineEvent, bool sync)
        {
            if (!_serviceBusConfiguration.HasInbox())
            {
                return;
            }

            var processorThreadPool = new ProcessorThreadPool(
                "InboxProcessor",
                _serviceBusOptions.Inbox.ThreadCount,
                new InboxProcessorFactory(_serviceBusOptions, _serviceBusConfiguration, _serviceBus, _workerAvailabilityService, _pipelineFactory, _pipelineThreadActivity),
                _serviceBusOptions.ProcessorThread);

            pipelineEvent.Pipeline.State.Add("InboxThreadPool", sync ? processorThreadPool.Start() : await processorThreadPool.StartAsync());

            await Task.CompletedTask.ConfigureAwait(false);
        }

        public void Execute(OnStartOutboxProcessing pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnStartOutboxProcessing pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnStartOutboxProcessing pipelineEvent, bool sync)
        {
            if (!_serviceBusConfiguration.HasOutbox())
            {
                return;
            }

            var processorThreadPool = new ProcessorThreadPool(
                "OutboxProcessor",
                _serviceBusOptions.Outbox.ThreadCount,
                new OutboxProcessorFactory(_serviceBusOptions, _pipelineFactory, _pipelineThreadActivity),
                _serviceBusOptions.ProcessorThread);

            pipelineEvent.Pipeline.State.Add("OutboxThreadPool", sync ? processorThreadPool.Start() : await processorThreadPool.StartAsync());

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}