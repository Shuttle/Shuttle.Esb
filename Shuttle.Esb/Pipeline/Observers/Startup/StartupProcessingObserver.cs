using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class StartupProcessingObserver :
        IPipelineObserver<OnStartInboxProcessing>,
        IPipelineObserver<OnStartControlInboxProcessing>,
        IPipelineObserver<OnStartOutboxProcessing>,
        IPipelineObserver<OnStartDeferredMessageProcessing>,
        IPipelineObserver<OnStartWorker>,
        IPipelineObserver<OnStarting>,
		IPipelineObserver<OnStarted>
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IServiceBusEvents _events;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IServiceBus _bus;
        private readonly IQueueManager _queueManager;
        private readonly IWorkerAvailabilityManager _workerAvailabilityManager;

        public StartupProcessingObserver(IServiceBus bus, IServiceBusConfiguration configuration, IServiceBusEvents events, IQueueManager queueManager, IWorkerAvailabilityManager workerAvailabilityManager, IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(bus, "bus");
            Guard.AgainstNull(configuration, "configuration");
            Guard.AgainstNull(events, "events");
            Guard.AgainstNull(queueManager, "queueManager");
            Guard.AgainstNull(workerAvailabilityManager, "workerAvailabilityManager");
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");

            _bus = bus;
            _queueManager = queueManager;
            _workerAvailabilityManager = workerAvailabilityManager;
            _pipelineFactory = pipelineFactory;
            _configuration = configuration;
            _events = events;
        }

        public void Execute(OnStartControlInboxProcessing pipelineEvent)
        {
            if (!_configuration.HasControlInbox)
            {
                return;
            }

            _configuration.ControlInbox.WorkQueue = _configuration.ControlInbox.WorkQueue ??
                _queueManager.CreateQueue(_configuration.ControlInbox.WorkQueueUri);

            _configuration.ControlInbox.ErrorQueue = _configuration.ControlInbox.ErrorQueue ??
                _queueManager.CreateQueue(_configuration.ControlInbox.ErrorQueueUri);

            pipelineEvent.Pipeline.State.Add(
                "ControlInboxThreadPool",
                new ProcessorThreadPool(
                    "ControlInboxProcessor",
                    _configuration.ControlInbox.ThreadCount,
                    new ControlInboxProcessorFactory(_configuration, _events, _pipelineFactory)).Start());
        }

        public void Execute(OnStartDeferredMessageProcessing pipelineEvent)
        {
            if (!_configuration.HasInbox || !_configuration.Inbox.HasDeferredQueue)
            {
                return;
            }

            _configuration.Inbox.DeferredMessageProcessor = new DeferredMessageProcessor(_pipelineFactory);

            pipelineEvent.Pipeline.State.Add(
                "DeferredMessageThreadPool",
                new ProcessorThreadPool(
                    "DeferredMessageProcessor",
                    1,
                    new DeferredMessageProcessorFactory(_configuration)).Start());
        }

        public void Execute(OnStartInboxProcessing pipelineEvent)
        {
            if (!_configuration.HasInbox)
            {
                return;
            }

            _configuration.Inbox.WorkQueue = _configuration.Inbox.WorkQueue ??
                                             _queueManager.CreateQueue(_configuration.Inbox.WorkQueueUri);

            _configuration.Inbox.DeferredQueue = _configuration.Inbox.DeferredQueue ?? (
                string.IsNullOrEmpty(_configuration.Inbox.DeferredQueueUri)
                    ? null
                    : _queueManager
                        .CreateQueue(_configuration.Inbox.DeferredQueueUri));

            _configuration.Inbox.ErrorQueue = _configuration.Inbox.ErrorQueue ??
                                              _queueManager.CreateQueue(_configuration.Inbox.ErrorQueueUri);

            pipelineEvent.Pipeline.State.Add(
                "InboxThreadPool",
                new ProcessorThreadPool(
                    "InboxProcessor",
                    _configuration.Inbox.ThreadCount,
                    new InboxProcessorFactory(_bus, _configuration, _events, _workerAvailabilityManager, _pipelineFactory))
                    .Start());
        }

        public void Execute(OnStartOutboxProcessing pipelineEvent)
        {
            if (!_configuration.HasOutbox)
            {
                return;
            }

            _configuration.Outbox.WorkQueue = _configuration.Outbox.WorkQueue ??
                                              _queueManager.CreateQueue(_configuration.Outbox.WorkQueueUri);

            _configuration.Outbox.ErrorQueue = _configuration.Outbox.ErrorQueue ??
                                               _queueManager.CreateQueue(_configuration.Outbox.ErrorQueueUri);

            pipelineEvent.Pipeline.State.Add(
                "OutboxThreadPool",
                new ProcessorThreadPool(
                    "OutboxProcessor",
                    _configuration.Outbox.ThreadCount,
                    new OutboxProcessorFactory(_configuration, _events, _pipelineFactory)).Start());
        }

        public void Execute(OnStartWorker pipelineEvent)
        {
            if (!_configuration.IsWorker)
            {
                return;
            }

            _configuration.Worker.DistributorControlInboxWorkQueue =
                _configuration.Worker.DistributorControlInboxWorkQueue ??
                _queueManager.CreateQueue(_configuration.Worker.DistributorControlInboxWorkQueueUri);

            _bus.Send(new WorkerStartedEvent
            {
                InboxWorkQueueUri = _configuration.Inbox.WorkQueue.Uri.ToString(),
                DateStarted = DateTime.Now
            },
                c => c.WithRecipient(_configuration.Worker.DistributorControlInboxWorkQueue));
        }

	    public void Execute(OnStarted pipelineEvent)
	    {
		    _events.OnStarted(this, new PipelineEventEventArgs(pipelineEvent));
	    }

	    public void Execute(OnStarting pipelineEvent)
	    {
			_events.OnStarting(this, new PipelineEventEventArgs(pipelineEvent));
		}
	}
}