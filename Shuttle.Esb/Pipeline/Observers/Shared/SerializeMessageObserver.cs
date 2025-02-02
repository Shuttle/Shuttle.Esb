using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;

namespace Shuttle.Esb;

public interface ISerializeMessageObserver : IPipelineObserver<OnSerializeMessage>
{
}

public class SerializeMessageObserver : ISerializeMessageObserver
{
    private readonly ISerializer _serializer;

    public SerializeMessageObserver(ISerializer serializer)
    {
        _serializer = Guard.AgainstNull(serializer);
    }

    public async Task ExecuteAsync(IPipelineContext<OnSerializeMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var message = Guard.AgainstNull(state.GetMessage());
        var transportMessage = Guard.AgainstNull(state.GetTransportMessage());

        await using (var stream = await _serializer.SerializeAsync(message).ConfigureAwait(false))
        {
            transportMessage.Message = await stream.ToBytesAsync().ConfigureAwait(false);
        }

        state.SetMessageBytes(transportMessage.Message);
    }
}