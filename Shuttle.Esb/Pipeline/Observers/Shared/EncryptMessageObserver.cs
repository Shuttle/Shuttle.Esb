using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IEncryptMessageObserver : IPipelineObserver<OnEncryptMessage>
    {
    }

    public class EncryptMessageObserver : IEncryptMessageObserver
    {
        private readonly IServiceBusConfiguration _configuration;

        public EncryptMessageObserver(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

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

            if (algorithm == null)
            {
                throw new InvalidOperationException(
                    string.Format(Resources.MissingEncryptionAlgorithmException,
                        transportMessage.EncryptionAlgorithm));
            }

            transportMessage.Message = algorithm.Encrypt(transportMessage.Message);
        }
    }
}