using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class DeferTransportMessageObserver : IPipelineObserver<OnAfterDeserializeTransportMessage>
	{
	    private readonly IServiceBusConfiguration _configuration;
	    private readonly ILog _log;

		public DeferTransportMessageObserver(IServiceBusConfiguration configuration)
		{
            Guard.AgainstNull(configuration, "configuration");

		    _configuration = configuration;
		    _log = Log.For(this);
		}

	    public void Execute(OnAfterDeserializeTransportMessage pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var receivedMessage = state.GetReceivedMessage();
			var transportMessage = state.GetTransportMessage();
			var workQueue = state.GetWorkQueue();

			Guard.AgainstNull(receivedMessage, "receivedMessage");
			Guard.AgainstNull(transportMessage, "transportMessage");
			Guard.AgainstNull(workQueue, "workQueue");

			if (!transportMessage.IsIgnoring())
			{
				return;
			}

			using (var stream = receivedMessage.Stream.Copy())
			{
				if (state.GetDeferredQueue() == null)
				{
					workQueue.Enqueue(transportMessage, stream);
				}
				else
				{
					state.GetDeferredQueue().Enqueue(transportMessage, stream);

					_configuration.Inbox.DeferredMessageProcessor.MessageDeferred(transportMessage.IgnoreTillDate);
				}
			}

			workQueue.Acknowledge(receivedMessage.AcknowledgementToken);

			if (_log.IsTraceEnabled)
			{
				_log.Trace(string.Format(EsbResources.TraceTransportMessageDeferred, transportMessage.MessageId,
					transportMessage.IgnoreTillDate.ToString("O")));
			}

			pipelineEvent.Pipeline.Abort();
		}
	}
}