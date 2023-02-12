using System;
using System.Threading.Tasks;
using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface ICompressMessageObserver : IPipelineObserver<OnCompressMessage>
    {
    }

    public class CompressMessageObserver : ICompressMessageObserver
    {
        private readonly ICompressionService _compressionService;

        public CompressMessageObserver(ICompressionService compressionService)
        {
            Guard.AgainstNull(compressionService, nameof(compressionService));

            _compressionService = compressionService;
        }

        public async Task Execute(OnCompressMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();

            if (!transportMessage.CompressionEnabled())
            {
                return;
            }

            transportMessage.Message = await _compressionService.Compress(transportMessage.CompressionAlgorithm, transportMessage.Message).ConfigureAwait(false);
        }
    }
}