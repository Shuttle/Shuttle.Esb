using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class DispatchTransportMessageObserver : IPipelineObserver<OnDispatchTransportMessage>
	{
		private readonly ILog _log;

		public DispatchTransportMessageObserver()
		{
			_log = Log.For(this);
		}

		public void Execute(OnDispatchTransportMessage pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var transportMessage = state.GetTransportMessage();
			var transportMessageReceived = state.GetTransportMessageReceived();
			var bus = state.GetServiceBus();

			if (transportMessageReceived != null && bus.Configuration.HasIdempotenceService)
			{
				try
				{
					bus.Configuration.IdempotenceService.AddDeferredMessage(transportMessageReceived, transportMessage, state.GetTransportMessageStream());
				}
				catch (Exception ex)
				{
					bus.Configuration.IdempotenceService.AccessException(_log, ex, pipelineEvent.Pipeline);
				}

				return;
			}

			Guard.AgainstNull(transportMessage, "transportMessage");
			Guard.AgainstNullOrEmptyString(transportMessage.RecipientInboxWorkQueueUri, "uri");

			if (transportMessage.IsIgnoring() && bus.Configuration.HasInbox && bus.Configuration.Inbox.HasDeferredQueue)
			{
				bus.Configuration.Inbox.DeferredQueue.Enqueue(transportMessage.MessageId, state.GetTransportMessageStream());
				bus.Configuration.Inbox.DeferredMessageProcessor.MessageDeferred(transportMessage.IgnoreTillDate);

				return;
			}

			var queue = !bus.Configuration.HasOutbox
				            ? bus.Configuration.QueueManager.GetQueue(transportMessage.RecipientInboxWorkQueueUri)
				            : bus.Configuration.Outbox.WorkQueue;

			using (var stream = state.GetTransportMessageStream().Copy())
			{
				queue.Enqueue(transportMessage.MessageId, stream);
			}
		}
	}
}