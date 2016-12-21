using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class EncryptMessageObserver : IPipelineObserver<OnEncryptMessage>
	{
        private readonly IServiceBusConfiguration _configuration;

        public EncryptMessageObserver(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, "configuration");

            _configuration = configuration;
        }

        public void Execute(OnEncryptMessage pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var transportMessage = state.GetTransportMessage();

			if (!transportMessage.EncryptionEnabled())
			{
				return;
			}

			var algorithm = _configuration.FindEncryptionAlgorithm(transportMessage.EncryptionAlgorithm);

			Guard.Against<InvalidOperationException>(algorithm == null,
				string.Format(InfrastructureResources.MissingEncryptionAlgorithmException, transportMessage.CompressionAlgorithm));

			transportMessage.Message = algorithm.Encrypt(transportMessage.Message);
		}
	}
}