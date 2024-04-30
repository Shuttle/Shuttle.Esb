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

        public void Execute(OnCompressMessage pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnCompressMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false);
        }

        private async Task ExecuteAsync(OnCompressMessage pipelineEvent, bool sync)
        {
            var transportMessage = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State.GetTransportMessage();

            if (!transportMessage.CompressionEnabled())
            {
                return;
            }

            transportMessage.Message = sync
            ? _compressionService.Compress(transportMessage.CompressionAlgorithm, transportMessage.Message)
            : await _compressionService.CompressAsync(transportMessage.CompressionAlgorithm, transportMessage.Message).ConfigureAwait(false);
        }
    }
}