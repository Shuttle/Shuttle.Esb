using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public interface IIdempotenceObserver :
    IPipelineObserver<OnProcessIdempotenceMessage>,
    IPipelineObserver<OnIdempotenceMessageHandled>
{
}

public class IdempotenceObserver : IIdempotenceObserver
{
    private readonly IIdempotenceService _idempotenceService;

    public IdempotenceObserver(IIdempotenceService idempotenceService)
    {
        _idempotenceService = Guard.AgainstNull(idempotenceService);
    }

    public async Task ExecuteAsync(IPipelineContext<OnIdempotenceMessageHandled> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;

        if (state.GetProcessingStatus() == ProcessingStatus.Ignore)
        {
            return;
        }

        await _idempotenceService.MessageHandledAsync(Guard.AgainstNull(state.GetTransportMessage()));
    }

    public async Task ExecuteAsync(IPipelineContext<OnProcessIdempotenceMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;

        if (state.GetProcessingStatus() == ProcessingStatus.Ignore)
        {
            return;
        }

        state.SetProcessingStatus(await _idempotenceService.ProcessingStatusAsync(Guard.AgainstNull(state.GetTransportMessage())));
    }
}