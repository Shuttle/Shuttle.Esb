using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public interface IStartupProcessingObserver : 
        IPipelineObserver<OnStartInboxProcessing>, 
        IPipelineObserver<OnStartControlInboxProcessing>, 
        IPipelineObserver<OnStartOutboxProcessing>, 
        IPipelineObserver<OnStartDeferredMessageProcessing>, 
        IPipelineObserver<OnStarting>, 
        IPipelineObserver<OnStarted>
    {
    }

    public class StartupProcessingObserver : IStartupProcessingObserver
    {
        private readonly IServiceBus _bus;
        private readonly IServiceBusConfiguration _configuration;
        private readonly IServiceBusEvents _events;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IWorkerAvailabilityService _workerAvailabilityService;

        private static readonly TimeSpan JoinTimeout = TimeSpan.FromSeconds(1);

        public StartupProcessingObserver(IServiceBus bus, IServiceBusConfiguration configuration,
            IServiceBusEvents events, IWorkerAvailabilityService workerAvailabilityService,
            IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(bus, nameof(bus));
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(events, nameof(events));
            Guard.AgainstNull(workerAvailabilityService, nameof(workerAvailabilityService));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _bus = bus;
            _workerAvailabilityService = workerAvailabilityService;
            _pipelineFactory = pipelineFactory;
            _configuration = configuration;
            _events = events;
        }

        public void Execute(OnStartControlInboxProcessing pipelineEvent)
        {
            if (!_configuration.HasControl)
            {
                return;
            }

            pipelineEvent.Pipeline.State.Add(
                "ControlInboxThreadPool",
                new ProcessorThreadPool(
                    "ControlInboxProcessor",
                    _configuration.Control.ThreadCount,
                    JoinTimeout,
                    new ControlInboxProcessorFactory(_configuration, _events, _pipelineFactory)).Start());
        }

        public void Execute(OnStartDeferredMessageProcessing pipelineEvent)
        {
            if (!_configuration.HasInbox || !_configuration.Inbox.HasDeferredBrokerEndpoint)
            {
                return;
            }

            _configuration.Inbox.DeferredMessageProcessor = new DeferredMessageProcessor(_pipelineFactory);

            pipelineEvent.Pipeline.State.Add(
                "DeferredMessageThreadPool",
                new ProcessorThreadPool(
                    "DeferredMessageProcessor",
                    1,
                    JoinTimeout,
                    new DeferredMessageProcessorFactory(_configuration)).Start());
        }

        public void Execute(OnStarted pipelineEvent)
        {
            _events.OnStarted(this, new PipelineEventEventArgs(pipelineEvent));
        }

        public void Execute(OnStartInboxProcessing pipelineEvent)
        {
            if (!_configuration.HasInbox)
            {
                return;
            }

            pipelineEvent.Pipeline.State.Add(
                "InboxThreadPool",
                new ProcessorThreadPool(
                        "InboxProcessor",
                        _configuration.Inbox.ThreadCount,
                        JoinTimeout,
                        new InboxProcessorFactory(_bus, _configuration, _events, _workerAvailabilityService,
                            _pipelineFactory))
                    .Start());
        }

        public void Execute(OnStarting pipelineEvent)
        {
            _events.OnStarting(this, new PipelineEventEventArgs(pipelineEvent));
        }

        public void Execute(OnStartOutboxProcessing pipelineEvent)
        {
            if (!_configuration.HasOutbox)
            {
                return;
            }

            pipelineEvent.Pipeline.State.Add(
                "OutboxThreadPool",
                new ProcessorThreadPool(
                    "OutboxProcessor",
                    _configuration.Outbox.ThreadCount,
                    JoinTimeout,
                    new OutboxProcessorFactory(_configuration, _events, _pipelineFactory)).Start());
        }
    }
}