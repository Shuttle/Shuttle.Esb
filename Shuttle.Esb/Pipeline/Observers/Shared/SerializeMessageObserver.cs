using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class SerializeMessageObserver : IPipelineObserver<OnSerializeMessage>
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
            var bytes = _serializer.Serialize(message).ToBytes();

            state.SetMessageBytes(bytes);

            transportMessage.Message = bytes;
        }
    }
}