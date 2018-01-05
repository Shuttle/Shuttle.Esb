using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;

namespace Shuttle.Esb
{
    public interface ISerializeMessageObserver : IPipelineObserver<OnSerializeMessage>
    {
    }

    public class SerializeMessageObserver : ISerializeMessageObserver
    {
        private readonly ISerializer _serializer;

        public SerializeMessageObserver(ISerializer serializer)
        {
            Guard.AgainstNull(serializer, nameof(serializer));

            _serializer = serializer;
        }

        public void Execute(OnSerializeMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var message = state.GetMessage();
            var transportMessage = state.GetTransportMessage();

            using (var stream = _serializer.Serialize(message))
            {
                transportMessage.Message = stream.ToBytes();
                state.SetMessageBytes(transportMessage.Message);
            }
        }
    }
}