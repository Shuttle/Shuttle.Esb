using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public interface IEncryptMessageObserver : IPipelineObserver<OnEncryptMessage>
{
}

public class EncryptMessageObserver : IEncryptMessageObserver
{
    private readonly IEncryptionService _encryptionService;

    public EncryptMessageObserver(IEncryptionService encryptionService)
    {
        _encryptionService = Guard.AgainstNull(encryptionService);
    }

    public async Task ExecuteAsync(IPipelineContext<OnEncryptMessage> pipelineContext)
    {
        var transportMessage = Guard.AgainstNull(Guard.AgainstNull(pipelineContext).Pipeline.State.GetTransportMessage());

        if (!transportMessage.EncryptionEnabled())
        {
            return;
        }

        transportMessage.Message = await _encryptionService.EncryptAsync(transportMessage.EncryptionAlgorithm, transportMessage.Message).ConfigureAwait(false);
    }
}