using System.Threading.Tasks;
using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public interface ICompressMessageObserver : IPipelineObserver<OnCompressMessage>
{
}

public class CompressMessageObserver : ICompressMessageObserver
{
    private readonly ICompressionService _compressionService;

    public CompressMessageObserver(ICompressionService compressionService)
    {
        _compressionService = Guard.AgainstNull(compressionService);
    }

    public async Task ExecuteAsync(IPipelineContext<OnCompressMessage> pipelineContext)
    {
        var transportMessage = Guard.AgainstNull(Guard.AgainstNull(pipelineContext).Pipeline.State.GetTransportMessage());

        if (!transportMessage.CompressionEnabled())
        {
            return;
        }

        transportMessage.Message = await _compressionService.CompressAsync(transportMessage.CompressionAlgorithm, transportMessage.Message).ConfigureAwait(false);
    }
}