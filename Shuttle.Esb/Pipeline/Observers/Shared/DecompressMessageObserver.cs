using System.Threading.Tasks;
using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public interface IDecompressMessageObserver : IPipelineObserver<OnDecompressMessage>
{
}

public class DecompressMessageObserver : IDecompressMessageObserver
{
    private readonly ICompressionService _compressionService;

    public DecompressMessageObserver(ICompressionService compressionService)
    {
        _compressionService = Guard.AgainstNull(compressionService);
    }

    public async Task ExecuteAsync(IPipelineContext<OnDecompressMessage> pipelineContext)
    {
        var transportMessage = Guard.AgainstNull(Guard.AgainstNull(pipelineContext).Pipeline.State.GetTransportMessage());

        if (!transportMessage.CompressionEnabled())
        {
            return;
        }

        transportMessage.Message = await _compressionService.DecompressAsync(transportMessage.CompressionAlgorithm, transportMessage.Message).ConfigureAwait(false);
    }
}