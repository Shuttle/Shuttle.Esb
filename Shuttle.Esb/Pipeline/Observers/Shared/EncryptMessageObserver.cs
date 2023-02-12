using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IEncryptMessageObserver : IPipelineObserver<OnEncryptMessage>
    {
    }

    public class EncryptMessageObserver : IEncryptMessageObserver
    {
        private readonly IEncryptionService _encryptionService;

        public EncryptMessageObserver(IEncryptionService encryptionService)
        {
            Guard.AgainstNull(encryptionService, nameof(encryptionService));

            _encryptionService = encryptionService;
        }

        public async Task Execute(OnEncryptMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();

            if (!transportMessage.EncryptionEnabled())
            {
                return;
            }

            transportMessage.Message = await _encryptionService.Encrypt(transportMessage.EncryptionAlgorithm, transportMessage.Message).ConfigureAwait(false);
        }
    }
}