using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IDecompressMessageObserver : IPipelineObserver<OnDecompressMessage>
    {
    }

    public class DecompressMessageObserver : IDecompressMessageObserver
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

            if (algorithm == null)
            {
                throw new InvalidOperationException(
                    string.Format(Resources.MissingCompressionAlgorithmException,
                        transportMessage.CompressionAlgorithm));
            }

            transportMessage.Message = algorithm.Decompress(transportMessage.Message);
        }
    }
}