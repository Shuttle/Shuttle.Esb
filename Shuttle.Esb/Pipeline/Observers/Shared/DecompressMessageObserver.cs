using System;
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
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();

            if (!transportMessage.CompressionEnabled())
            {
                return;
            }

            transportMessage.Message = _compressionService.Decompress(transportMessage.CompressionAlgorithm, transportMessage.Message);
        }
    }
}