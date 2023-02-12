using System.Threading.Tasks;
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

        public async Task Execute(OnSerializeTransportMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();

            Guard.AgainstNull(transportMessage, nameof(transportMessage));

            state.SetTransportMessageStream(await _serializer.Serialize(transportMessage).ConfigureAwait(false));
        }
    }
}