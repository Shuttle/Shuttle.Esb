using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb
{
    public interface ISerializeTransportMessageObserver : IPipelineObserver<OnSerializeTransportMessage>
    {
    }

    public class SerializeTransportMessageObserver : ISerializeTransportMessageObserver
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