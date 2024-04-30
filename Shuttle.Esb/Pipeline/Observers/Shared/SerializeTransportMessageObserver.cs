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

        private async Task ExecuteAsync(OnSerializeTransportMessage pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var transportMessage = state.GetTransportMessage();

            Guard.AgainstNull(transportMessage, nameof(transportMessage));

            if (sync)
            {
                state.SetTransportMessageStream(_serializer.Serialize(transportMessage));
            }
            else
            {
                state.SetTransportMessageStream(await _serializer.SerializeAsync(transportMessage).ConfigureAwait(false));
            }
        }

        public void Execute(OnSerializeTransportMessage pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnSerializeTransportMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }
    }
}