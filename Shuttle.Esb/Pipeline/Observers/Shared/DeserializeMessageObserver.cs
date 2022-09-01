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
        event EventHandler<DeserializationExceptionEventArgs> MessageDeserializationException;
    }

    public class DeserializeMessageObserver : IDeserializeMessageObserver
    {
        private readonly ISerializer _serializer;

        public DeserializeMessageObserver(ISerializer serializer)
        {
            Guard.AgainstNull(serializer, nameof(serializer));

            _serializer = serializer;
        }

        public void Execute(OnDeserializeMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            Guard.AgainstNull(state.GetTransportMessage(), "transportMessage");
            Guard.AgainstNull(state.GetWorkQueue(), "workQueue");

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
                MessageDeserializationException.Invoke(this,
                    new DeserializationExceptionEventArgs(pipelineEvent, state.GetWorkQueue(), state.GetErrorQueue(), ex));

                if (state.GetWorkQueue() == null)
                {
                    throw;
                }

                transportMessage.RegisterFailure(ex.AllMessages(), new TimeSpan());

                state.GetErrorQueue().Enqueue(transportMessage, _serializer.Serialize(transportMessage));
                state.GetWorkQueue().Acknowledge(state.GetReceivedMessage().AcknowledgementToken);

                state.SetTransactionComplete();
                pipelineEvent.Pipeline.Abort();

                return;
            }

            state.SetMessage(message);
        }

        public event EventHandler<DeserializationExceptionEventArgs> MessageDeserializationException = delegate
        {
        };
    }
}