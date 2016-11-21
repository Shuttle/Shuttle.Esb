using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class DispatchTransportMessageObserver : IPipelineObserver<OnDispatchTransportMessage>
	{
	    private readonly IServiceBusConfiguration _configuration;
	    private readonly IQueueManager _queueManager;
	    private readonly IIdempotenceService _idempotenceService;
	    private readonly ILog _log;

		public DispatchTransportMessageObserver(IServiceBusConfiguration configuration, IQueueManager queueManager, IIdempotenceService idempotenceService)
		{
            Guard.AgainstNull(configuration, "configuration");
            Guard.AgainstNull(queueManager, "queueManager");
            Guard.AgainstNull(idempotenceService, "idempotenceService");

		    _configuration = configuration;
		    _queueManager = queueManager;
		    _idempotenceService = idempotenceService;
		    _log = Log.For(this);
		}

	    public void Execute(OnDispatchTransportMessage pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var transportMessage = state.GetTransportMessage();
			var transportMessageReceived = state.GetTransportMessageReceived();

			if (transportMessageReceived != null)
			{
				try
				{
				    if (_idempotenceService.AddDeferredMessage(transportMessageReceived, transportMessage,
				        state.GetTransportMessageStream()))
				    {
                        return;
                    }
				}
				catch (Exception ex)
				{
					_idempotenceService.AccessException(_log, ex, pipelineEvent.Pipeline);
				}
			}

			Guard.AgainstNull(transportMessage, "transportMessage");
			Guard.AgainstNullOrEmptyString(transportMessage.RecipientInboxWorkQueueUri, "uri");

			if (transportMessage.IsIgnoring() && _configuration.HasInbox && _configuration.Inbox.HasDeferredQueue)
			{
				_configuration.Inbox.DeferredQueue.Enqueue(transportMessage, state.GetTransportMessageStream());
				_configuration.Inbox.DeferredMessageProcessor.MessageDeferred(transportMessage.IgnoreTillDate);

				return;
			}

			var queue = !_configuration.HasOutbox
				? _queueManager.GetQueue(transportMessage.RecipientInboxWorkQueueUri)
				: _configuration.Outbox.WorkQueue;

			using (var stream = state.GetTransportMessageStream().Copy())
			{
				queue.Enqueue(transportMessage, stream);
			}
		}
	}
}