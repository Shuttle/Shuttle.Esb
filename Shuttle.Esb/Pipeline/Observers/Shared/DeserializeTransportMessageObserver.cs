using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;

namespace Shuttle.Esb
{
    public interface IDeserializeTransportMessageObserver : IPipelineObserver<OnDeserializeTransportMessage>
    {
    }

    public class DeserializeTransportMessageObserver : IDeserializeTransportMessageObserver
    {
        private readonly IServiceBusEvents _events;
        private readonly ILog _log;
        private readonly ISerializer _serializer;

        public DeserializeTransportMessageObserver(IServiceBusEvents events, ISerializer serializer)
        {
            Guard.AgainstNull(events, nameof(events));
            Guard.AgainstNull(serializer, nameof(serializer));

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

            Guard.AgainstNull(receivedMessage, nameof(receivedMessage));
            Guard.AgainstNull(workQueue, nameof(workQueue));
            Guard.AgainstNull(errorQueue, nameof(errorQueue));

            TransportMessage transportMessage;

            try
            {
                using (var stream = receivedMessage.Stream.Copy())
                {
                    transportMessage =
                        (TransportMessage) _serializer.Deserialize(typeof(TransportMessage), stream);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                _log.Error(string.Format(Resources.TransportMessageDeserializationException, workQueue.Uri.Secured(),
                    ex));

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