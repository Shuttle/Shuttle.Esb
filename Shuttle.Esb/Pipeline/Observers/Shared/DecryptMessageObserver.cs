using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IDecryptMessageObserver : IPipelineObserver<OnDecryptMessage>
    {
    }

    public class DecryptMessageObserver : IDecryptMessageObserver
    {
        private readonly IEncryptionService _encryptionService;

        public DecryptMessageObserver(IEncryptionService encryptionService)
        {
            Guard.AgainstNull(encryptionService, nameof(encryptionService));

            _encryptionService = encryptionService;
        }

        public void Execute(OnDecryptMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();

            if (!transportMessage.EncryptionEnabled())
            {
                return;
            }

            transportMessage.Message = _encryptionService.Decrypt(transportMessage.EncryptionAlgorithm, transportMessage.Message);
        }
    }
}