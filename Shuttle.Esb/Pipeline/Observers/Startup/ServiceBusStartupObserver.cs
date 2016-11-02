using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class ServiceBusStartupObserver :
		IPipelineObserver<OnCreateQueues>,
		IPipelineObserver<OnRegisterMessageHandlers>,
		IPipelineObserver<OnStartInboxProcessing>,
		IPipelineObserver<OnStartControlInboxProcessing>,
		IPipelineObserver<OnStartOutboxProcessing>,
		IPipelineObserver<OnStartDeferredMessageProcessing>,
		IPipelineObserver<OnStartWorker>
	{
		private readonly IServiceBus _bus;
		private readonly IServiceBusConfiguration _configuration;

		private readonly ILog _log;

		public ServiceBusStartupObserver(IServiceBus bus)
		{
			Guard.AgainstNull(bus, "bus");

			_bus = bus;
			_configuration = _bus.Configuration;
			_log = Log.For(this);
		}

		public void Execute(OnCreateQueues pipelineEvent)
		{
			if (!_configuration.CreateQueues)
			{
				return;
			}

			_configuration.QueueManager.CreatePhysicalQueues(_configuration);
		}

		public void Execute(OnRegisterMessageHandlers pipelineEvent)
		{
		    if (_bus.Configuration.RegisterHandlers)
		    {
		        _configuration.Container.RegisterMessageHandlers();
		    }
		}

		public void Execute(OnStartControlInboxProcessing pipelineEvent)
		{
			if (!_configuration.HasControlInbox)
			{
				return;
			}

			pipelineEvent.Pipeline.State.Add(
				"ControlInboxThreadPool",
				new ProcessorThreadPool(
					"ControlInboxProcessor",
					_configuration.ControlInbox.ThreadCount,
					new ControlInboxProcessorFactory(_bus)).Start());
		}

		public void Execute(OnStartDeferredMessageProcessing pipelineEvent)
		{
			if (!_configuration.HasInbox || !_configuration.Inbox.HasDeferredQueue)
			{
				return;
			}

			_configuration.Inbox.DeferredMessageProcessor = new DeferredMessageProcessor(_bus);

			pipelineEvent.Pipeline.State.Add(
				"DeferredMessageThreadPool",
				new ProcessorThreadPool(
					"DeferredMessageProcessor",
					1,
					new DeferredMessageProcessorFactory(_bus)).Start());
		}

		public void Execute(OnStartInboxProcessing pipelineEvent)
		{
			if (!_configuration.HasInbox)
			{
				return;
			}

			var inbox = _configuration.Inbox;

			pipelineEvent.Pipeline.State.Add(
				"InboxThreadPool",
				new ProcessorThreadPool(
					"InboxProcessor",
					inbox.ThreadCount,
					new InboxProcessorFactory(_bus)).Start());
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
					new OutboxProcessorFactory(_bus)).Start());
		}

		public void Execute(OnStartWorker pipelineEvent)
		{
			if (!_configuration.IsWorker)
			{
				return;
			}

			_bus.Send(new WorkerStartedEvent
			{
				InboxWorkQueueUri = _configuration.Inbox.WorkQueue.Uri.ToString(),
				DateStarted = DateTime.Now
			},
				c => c.WithRecipient(_configuration.Worker.DistributorControlInboxWorkQueue));
		}
	}
}