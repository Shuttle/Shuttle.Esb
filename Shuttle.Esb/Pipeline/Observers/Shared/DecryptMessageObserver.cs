using System.Threading.Tasks;
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
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnDecryptMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false);
        }

        private async Task ExecuteAsync(OnDecryptMessage pipelineEvent, bool sync)
        {
            var transportMessage = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State.GetTransportMessage();

            if (!transportMessage.EncryptionEnabled())
            {
                return;
            }

            transportMessage.Message = sync
                ? _encryptionService.Decrypt(transportMessage.EncryptionAlgorithm, transportMessage.Message)
                : await _encryptionService.DecryptAsync(transportMessage.EncryptionAlgorithm, transportMessage.Message).ConfigureAwait(false);
        }
    }
}