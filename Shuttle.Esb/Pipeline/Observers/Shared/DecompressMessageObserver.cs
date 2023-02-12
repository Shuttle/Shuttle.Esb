using System;
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

        public async Task Execute(OnDecompressMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();

            if (!transportMessage.CompressionEnabled())
            {
                return;
            }

            transportMessage.Message = await _compressionService.Decompress(transportMessage.CompressionAlgorithm, transportMessage.Message).ConfigureAwait(false);
        }
    }
}