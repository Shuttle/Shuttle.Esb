using System.Threading.Tasks;
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

        public async Task Execute(OnSerializeMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var message = state.GetMessage();
            var transportMessage = state.GetTransportMessage();

            await using (var stream = await _serializer.Serialize(message).ConfigureAwait(false))
            {
                transportMessage.Message = await stream.ToBytesAsync().ConfigureAwait(false);
                state.SetMessageBytes(transportMessage.Message);
            }
        }
    }
}