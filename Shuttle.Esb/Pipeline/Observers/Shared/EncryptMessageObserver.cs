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

        private async Task ExecuteAsync(OnEncryptMessage pipelineEvent, bool sync)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();

            if (!transportMessage.EncryptionEnabled())
            {
                return;
            }

            if (sync)
            {
                transportMessage.Message = _encryptionService.Encrypt(transportMessage.EncryptionAlgorithm, transportMessage.Message);
            }
            else
            {
                transportMessage.Message = await _encryptionService.EncryptAsync(transportMessage.EncryptionAlgorithm, transportMessage.Message).ConfigureAwait(false);
            }
        }

        public void Execute(OnEncryptMessage pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnEncryptMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }
    }
}