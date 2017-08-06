using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DecryptMessageObserver : IPipelineObserver<OnDecryptMessage>
    {
        private readonly IServiceBusConfiguration _configuration;

        public DecryptMessageObserver(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;
        }

        public void Execute(OnDecryptMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();

            if (!transportMessage.EncryptionEnabled())
            {
                return;
            }

            var algorithm = _configuration.FindEncryptionAlgorithm(transportMessage.EncryptionAlgorithm);

            Guard.Against<InvalidOperationException>(algorithm == null,
                string.Format(InfrastructureResources.MissingEncryptionAlgorithmException,
                    transportMessage.EncryptionAlgorithm));

            transportMessage.Message = algorithm.Decrypt(transportMessage.Message);
        }
    }
}