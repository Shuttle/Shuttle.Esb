using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransactionScope;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb;

public interface ISendDeferredObserver :
    IPipelineObserver<OnSendDeferred>,
    IPipelineObserver<OnAfterSendDeferred>
{
}

public class SendDeferredObserver : ISendDeferredObserver
{
    private readonly IIdempotenceService _idempotenceService;
    private readonly IPipelineFactory _pipelineFactory;
    private readonly ISerializer _serializer;

    public SendDeferredObserver(IPipelineFactory pipelineFactory, ISerializer serializer,
        IIdempotenceService idempotenceService)
    {
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
        _serializer = Guard.AgainstNull(serializer);
        _idempotenceService = Guard.AgainstNull(idempotenceService);
    }

    public async Task ExecuteAsync(IPipelineContext<OnAfterSendDeferred> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;

        if (state.GetProcessingStatus() == ProcessingStatus.Ignore ||
            (
                pipelineContext.Pipeline.Exception != null && !state.GetTransactionScopeCompleted()
            ))
        {
            return;
        }

        var transportMessage = Guard.AgainstNull(state.GetTransportMessage());

        await _idempotenceService.ProcessingCompletedAsync(transportMessage).ConfigureAwait(false);
    }

    public async Task ExecuteAsync(IPipelineContext<OnSendDeferred> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;

        if (state.GetProcessingStatus() == ProcessingStatus.Ignore)
        {
            return;
        }

        var transportMessage = Guard.AgainstNull(state.GetTransportMessage());

        var deferredMessages = await _idempotenceService.GetDeferredMessagesAsync(transportMessage).ConfigureAwait(false);

        foreach (var stream in deferredMessages)
        {
            var deferredTransportMessage = (TransportMessage)await _serializer.DeserializeAsync(typeof(TransportMessage), stream).ConfigureAwait(false);

            var messagePipeline = _pipelineFactory.GetPipeline<DispatchTransportMessagePipeline>();

            try
            {
               await messagePipeline.ExecuteAsync(deferredTransportMessage, null).ConfigureAwait(false);
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(messagePipeline);
            }

            await _idempotenceService.DeferredMessageSentAsync(transportMessage, deferredTransportMessage).ConfigureAwait(false);
        }
    }
}