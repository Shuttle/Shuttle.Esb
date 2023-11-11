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

        private async Task ExecuteAsync(OnSerializeMessage pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent,nameof(pipelineEvent)).Pipeline.State;
            var message = Guard.AgainstNull(state.GetMessage(), StateKeys.Message);
            var transportMessage = Guard.AgainstNull(state.GetTransportMessage(), StateKeys.TransportMessage);

            if (sync)
            {
                using (var stream = _serializer.Serialize(message))
                {
                    transportMessage.Message = stream.ToBytes();
                }
            }
            else
            {
                await using (var stream = await _serializer.SerializeAsync(message).ConfigureAwait(false))
                {
                    transportMessage.Message = await stream.ToBytesAsync().ConfigureAwait(false);
                }
            }

            state.SetMessageBytes(transportMessage.Message);
        }

        public void Execute(OnSerializeMessage pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnSerializeMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }
    }
}