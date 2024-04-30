using System.Threading.Tasks;
using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IDecompressMessageObserver : IPipelineObserver<OnDecompressMessage>
    {
    }

    public class DecompressMessageObserver : IDecompressMessageObserver
    {
        private readonly ICompressionService _compressionService;

        public DecompressMessageObserver(ICompressionService compressionService)
        {
            Guard.AgainstNull(compressionService, nameof(compressionService));

            _compressionService = compressionService;
        }

        public void Execute(OnDecompressMessage pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnDecompressMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false);
        }

        private async Task ExecuteAsync(OnDecompressMessage pipelineEvent, bool sync)
        {
            var transportMessage = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State.GetTransportMessage();

            if (!transportMessage.CompressionEnabled())
            {
                return;
            }

            transportMessage.Message = sync
                ? _compressionService.Decompress(transportMessage.CompressionAlgorithm, transportMessage.Message)
                : await _compressionService.DecompressAsync(transportMessage.CompressionAlgorithm, transportMessage.Message).ConfigureAwait(false);
        }
    }
}