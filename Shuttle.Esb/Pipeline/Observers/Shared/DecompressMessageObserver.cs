using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DecompressMessageObserver : IPipelineObserver<OnDecompressMessage>
    {
        private readonly IServiceBusConfiguration _configuration;

        public DecompressMessageObserver(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;
        }

        public void Execute(OnDecompressMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();

            if (!transportMessage.CompressionEnabled())
            {
                return;
            }

            var algorithm = _configuration.FindCompressionAlgorithm(transportMessage.CompressionAlgorithm);

            Guard.Against<InvalidOperationException>(algorithm == null,
                string.Format(InfrastructureResources.MissingCompressionAlgorithmException,
                    transportMessage.CompressionAlgorithm));

            transportMessage.Message = algorithm.Decompress(transportMessage.Message);
        }
    }
}