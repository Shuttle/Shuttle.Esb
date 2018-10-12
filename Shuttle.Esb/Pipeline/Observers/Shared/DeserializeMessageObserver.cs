using System;
using System.IO;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb
{
    public interface IDeserializeMessageObserver : IPipelineObserver<OnDeserializeMessage>
    {
    }

    public class DeserializeMessageObserver : IDeserializeMessageObserver
    {
        private readonly IServiceBusEvents _events;
        private readonly ISerializer _serializer;

        public DeserializeMessageObserver(IServiceBusEvents events, ISerializer serializer)
        {
            Guard.AgainstNull(events, nameof(events));
            Guard.AgainstNull(serializer, nameof(serializer));

            _events = events;
            _serializer = serializer;
        }

        public void Execute(OnDeserializeMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            Guard.AgainstNull(state.GetTransportMessage(), "transportMessage");
            Guard.AgainstNull(state.GetWorkQueue(), "workQueue");
            Guard.AgainstNull(state.GetErrorQueue(), "errorQueue");

            var transportMessage = state.GetTransportMessage();

            object message;

            try
            {
                var data = transportMessage.Message;
                using (var stream = new MemoryStream(data, 0, data.Length, false, true))
                {
                    message = _serializer.Deserialize(Type.GetType(transportMessage.AssemblyQualifiedName, true, true),
                        stream);
                }
            }
            catch (Exception ex)
            {
                transportMessage.RegisterFailure(ex.AllMessages(), new TimeSpan());

                state.GetErrorQueue().Enqueue(transportMessage, _serializer.Serialize(transportMessage));
                state.GetWorkQueue().Acknowledge(state.GetReceivedMessage().AcknowledgementToken);

                state.SetTransactionComplete();
                pipelineEvent.Pipeline.Abort();

                _events.OnMessageDeserializationException(this,
                    new DeserializationExceptionEventArgs(
                        pipelineEvent,
                        state.GetWorkQueue(),
                        state.GetErrorQueue(),
                        ex));

                return;
            }

            state.SetMessage(message);
        }
    }
}