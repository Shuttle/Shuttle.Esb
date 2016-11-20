using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class DispatchTransportMessageObserver : IPipelineObserver<OnDispatchTransportMessage>
	{
	    private readonly IQueueManager _queueManager;
	    private readonly IIdempotenceService _idempotenceService;
	    private readonly ILog _log;

		public DispatchTransportMessageObserver(IQueueManager queueManager, IIdempotenceService idempotenceService)
		{
            Guard.AgainstNull(queueManager, "queueManager");
            Guard.AgainstNull(idempotenceService, "idempotenceService");

		    _queueManager = queueManager;
		    _idempotenceService = idempotenceService;
		    _log = Log.For(this);
		}

	    public void Execute(OnDispatchTransportMessage pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var transportMessage = state.GetTransportMessage();
			var transportMessageReceived = state.GetTransportMessageReceived();
			var bus = state.GetServiceBus();

			if (transportMessageReceived != null && _idempotenceService.CanDeferMessage)
			{
				try
				{
					_idempotenceService.AddDeferredMessage(transportMessageReceived, transportMessage,
						state.GetTransportMessageStream());
				}
				catch (Exception ex)
				{
					_idempotenceService.AccessException(_log, ex, pipelineEvent.Pipeline);
				}

				return;
			}

			Guard.AgainstNull(transportMessage, "transportMessage");
			Guard.AgainstNullOrEmptyString(transportMessage.RecipientInboxWorkQueueUri, "uri");

			if (transportMessage.IsIgnoring() && bus.Configuration.HasInbox && bus.Configuration.Inbox.HasDeferredQueue)
			{
				bus.Configuration.Inbox.DeferredQueue.Enqueue(transportMessage, state.GetTransportMessageStream());
				bus.Configuration.Inbox.DeferredMessageProcessor.MessageDeferred(transportMessage.IgnoreTillDate);

				return;
			}

			var queue = !bus.Configuration.HasOutbox
				? _queueManager.GetQueue(transportMessage.RecipientInboxWorkQueueUri)
				: bus.Configuration.Outbox.WorkQueue;

			using (var stream = state.GetTransportMessageStream().Copy())
			{
				queue.Enqueue(transportMessage, stream);
			}
		}
	}
}