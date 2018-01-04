using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class CompressMessageObserver : IPipelineObserver<OnCompressMessage>
    {
        private readonly IServiceBusConfiguration _configuration;

        public CompressMessageObserver(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;
        }

        public void Execute(OnCompressMessage pipelineEvent)
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
                throw new InvalidOperationException(string.Format(Resources.MissingCompressionAlgorithmException,
                    transportMessage.CompressionAlgorithm));
            }

            transportMessage.Message = algorithm.Compress(transportMessage.Message);
        }
    }
}