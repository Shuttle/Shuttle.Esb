using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb;

public interface IShutdownProcessingObserver : IPipelineObserver<OnStopping>
{
}

public class ShutdownProcessingObserver : IShutdownProcessingObserver
{
    private readonly IQueueService _queueService;

    public ShutdownProcessingObserver(IQueueService queueService)
    {
        _queueService = Guard.AgainstNull(queueService);
    }

    public async Task ExecuteAsync(IPipelineContext<OnStopping> pipelineContext)
    {
        await _queueService.TryDisposeAsync();
    }
}