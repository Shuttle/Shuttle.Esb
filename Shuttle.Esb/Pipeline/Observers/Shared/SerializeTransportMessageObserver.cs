using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb;

public interface ISerializeTransportMessageObserver : IPipelineObserver<OnSerializeTransportMessage>
{
}

public class SerializeTransportMessageObserver : ISerializeTransportMessageObserver
{
    private readonly ISerializer _serializer;

    public SerializeTransportMessageObserver(ISerializer serializer)
    {
        _serializer = Guard.AgainstNull(serializer);
    }

    public async Task ExecuteAsync(IPipelineContext<OnSerializeTransportMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;

        state.SetTransportMessageStream(await _serializer.SerializeAsync(Guard.AgainstNull(state.GetTransportMessage())).ConfigureAwait(false));
    }
}