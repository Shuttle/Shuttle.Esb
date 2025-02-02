using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public interface IDecryptMessageObserver : IPipelineObserver<OnDecryptMessage>
{
}

public class DecryptMessageObserver : IDecryptMessageObserver
{
    private readonly IEncryptionService _encryptionService;

    public DecryptMessageObserver(IEncryptionService encryptionService)
    {
        _encryptionService = Guard.AgainstNull(encryptionService);
    }

    public async Task ExecuteAsync(IPipelineContext<OnDecryptMessage> pipelineContext)
    {
        var transportMessage = Guard.AgainstNull(Guard.AgainstNull(pipelineContext).Pipeline.State.GetTransportMessage());

        if (!transportMessage.EncryptionEnabled())
        {
            return;
        }

        transportMessage.Message = await _encryptionService.DecryptAsync(transportMessage.EncryptionAlgorithm, transportMessage.Message).ConfigureAwait(false);
    }
}