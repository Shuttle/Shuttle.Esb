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
        private readonly IDeferredMessageProcessor _deferredMessageProcessor;

        public StartupProcessingObserver(IOptions<ServiceBusOptions> serviceBusOptions, IServiceBus serviceBus,
            IServiceBusConfiguration serviceBusConfiguration, IWorkerAvailabilityService workerAvailabilityService, IDeferredMessageProcessor deferredMessageProcessor,
            IPipelineFactory pipelineFactory, IPipelineThreadActivity pipelineThreadActivity)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));

            _serviceBus = Guard.AgainstNull(serviceBus, nameof(serviceBus));
            _serviceBusOptions = Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));
            _workerAvailabilityService = Guard.AgainstNull(workerAvailabilityService, nameof(workerAvailabilityService));
            _deferredMessageProcessor = Guard.AgainstNull(deferredMessageProcessor, nameof(deferredMessageProcessor));
            _pipelineFactory = Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            _pipelineThreadActivity = Guard.AgainstNull(pipelineThreadActivity, nameof(pipelineThreadActivity));
            _serviceBusConfiguration = Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));
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

            var processorThreadPool = new ProcessorThreadPool(
                "DeferredMessageProcessor",
                1,
                new DeferredMessageProcessorFactory(_deferredMessageProcessor),
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