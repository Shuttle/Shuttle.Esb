using System;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public interface IStartupProcessingObserver : 
        IPipelineObserver<OnStartInboxProcessing>, 
        IPipelineObserver<OnStartControlInboxProcessing>, 
        IPipelineObserver<OnStartOutboxProcessing>, 
        IPipelineObserver<OnStartDeferredMessageProcessing>
    {
    }

    public class StartupProcessingObserver : IStartupProcessingObserver
    {
        private readonly IServiceBus _serviceBus;
        private readonly ServiceBusOptions _options;
        private readonly IServiceBusConfiguration _serviceBusConfiguration;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IPipelineThreadActivity _pipelineThreadActivity;
        private readonly IWorkerAvailabilityService _workerAvailabilityService;

        private static readonly TimeSpan JoinTimeout = TimeSpan.FromSeconds(1);

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
            _options = serviceBusOptions.Value;
            _workerAvailabilityService = workerAvailabilityService;
            _pipelineFactory = pipelineFactory;
            _pipelineThreadActivity = pipelineThreadActivity;
            _serviceBusConfiguration = serviceBusConfiguration;
        }

        public void Execute(OnStartControlInboxProcessing pipelineEvent)
        {
            if (!_serviceBusConfiguration.HasControlInbox())
            {
                return;
            }

            pipelineEvent.Pipeline.State.Add(
                "ControlInboxThreadPool",
                new ProcessorThreadPool(
                    "ControlInboxProcessor",
                    _options.ControlInbox.ThreadCount,
                    JoinTimeout,
                    new ControlInboxProcessorFactory(_options, _pipelineFactory, _pipelineThreadActivity)).Start());
        }

        public void Execute(OnStartDeferredMessageProcessing pipelineEvent)
        {
            if (!_serviceBusConfiguration.HasInbox() || !_serviceBusConfiguration.Inbox.HasDeferredQueue())
            {
                return;
            }

            _serviceBusConfiguration.Inbox.DeferredMessageProcessor = new DeferredMessageProcessor(_pipelineFactory);

            pipelineEvent.Pipeline.State.Add(
                "DeferredMessageThreadPool",
                new ProcessorThreadPool(
                    "DeferredMessageProcessor",
                    1,
                    JoinTimeout,
                    new DeferredMessageProcessorFactory(_serviceBusConfiguration)).Start());
        }

        public void Execute(OnStartInboxProcessing pipelineEvent)
        {
            if (!_serviceBusConfiguration.HasInbox())
            {
                return;
            }

            pipelineEvent.Pipeline.State.Add(
                "InboxThreadPool",
                new ProcessorThreadPool(
                        "InboxProcessor",
                        _options.Inbox.ThreadCount,
                        JoinTimeout,
                        new InboxProcessorFactory(_options, _serviceBusConfiguration, _serviceBus, _workerAvailabilityService, _pipelineFactory, _pipelineThreadActivity))
                    .Start());
        }

        public void Execute(OnStartOutboxProcessing pipelineEvent)
        {
            if (!_serviceBusConfiguration.HasOutbox())
            {
                return;
            }

            pipelineEvent.Pipeline.State.Add(
                "OutboxThreadPool",
                new ProcessorThreadPool(
                    "OutboxProcessor",
                    _options.Outbox.ThreadCount,
                    JoinTimeout,
                    new OutboxProcessorFactory(_options, _pipelineFactory, _pipelineThreadActivity)).Start());
        }
    }
}