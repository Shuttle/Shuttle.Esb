using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class DeserializeTransportMessageObserver : IPipelineObserver<OnDeserializeTransportMessage>
	{
		private readonly ILog _log;
	    private readonly ISerializer _serializer;
        private readonly IServiceBusEvents _events;

        public DeserializeTransportMessageObserver(IServiceBusEvents events, ISerializer serializer)
	    {
            Guard.AgainstNull(events, "events");
            Guard.AgainstNull(serializer, "serializer");

            _events = events;
            _serializer = serializer;
	        _log = Log.For(this);
	    }

	    public void Execute(OnDeserializeTransportMessage pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var receivedMessage = state.GetReceivedMessage();
			var workQueue = state.GetWorkQueue();
			var errorQueue = state.GetErrorQueue();

			Guard.AgainstNull(receivedMessage, "receivedMessage");
			Guard.AgainstNull(workQueue, "workQueue");
			Guard.AgainstNull(errorQueue, "errorQueue");

			TransportMessage transportMessage;

			try
			{
				using (var stream = receivedMessage.Stream.Copy())
				{
					transportMessage =
						(TransportMessage) _serializer.Deserialize(typeof (TransportMessage), stream);
				}
			}
			catch (Exception ex)
			{
				_log.Error(ex.ToString());
				_log.Error(string.Format(EsbResources.TransportMessageDeserializationException, workQueue.Uri, ex));

                state.GetWorkQueue().Acknowledge(state.GetReceivedMessage().AcknowledgementToken);

                _events.OnTransportMessageDeserializationException(this,
                    new DeserializationExceptionEventArgs(
                        pipelineEvent,
                        workQueue,
                        errorQueue,
                        ex));

                pipelineEvent.Pipeline.Abort();

				return;
			}

			state.SetTransportMessage(transportMessage);
			state.SetMessageBytes(transportMessage.Message);

			transportMessage.AcceptInvariants();
		}
	}
}