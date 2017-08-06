using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class SerializeTransportMessageObserver : IPipelineObserver<OnSerializeTransportMessage>
    {
        private readonly ISerializer _serializer;

        public SerializeTransportMessageObserver(ISerializer serializer)
        {
            Guard.AgainstNull(serializer, nameof(serializer));

            _serializer = serializer;
        }

        public void Execute(OnSerializeTransportMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();

            Guard.AgainstNull(transportMessage, nameof(transportMessage));

            state.SetTransportMessageStream(_serializer.Serialize(transportMessage));
        }
    }
}