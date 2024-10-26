using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb;

public interface IDispatchTransportMessageObserver : IPipelineObserver<OnDispatchTransportMessage>
{
}

public class DispatchTransportMessageObserver : IDispatchTransportMessageObserver
{
    private readonly IIdempotenceService _idempotenceService;
    private readonly IQueueService _queueService;
    private readonly IServiceBusConfiguration _serviceBusConfiguration;

    public DispatchTransportMessageObserver(IServiceBusConfiguration serviceBusConfiguration, IQueueService queueService, IIdempotenceService idempotenceService)
    {
        _serviceBusConfiguration = Guard.AgainstNull(serviceBusConfiguration);
        _queueService = Guard.AgainstNull(queueService);
        _idempotenceService = Guard.AgainstNull(idempotenceService);
    }

    public async Task ExecuteAsync(IPipelineContext<OnDispatchTransportMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var transportMessage = Guard.AgainstNull(state.GetTransportMessage());
        var transportMessageReceived = state.GetTransportMessageReceived();

        if (transportMessageReceived != null &&
            await _idempotenceService.AddDeferredMessageAsync(transportMessageReceived, transportMessage, Guard.AgainstNull(state.GetTransportMessageStream()))
           )
        {
            return;
        }

        Guard.AgainstNullOrEmptyString(transportMessage.RecipientInboxWorkQueueUri);

        var queue = !_serviceBusConfiguration.HasOutbox()
            ? _queueService.Get(transportMessage.RecipientInboxWorkQueueUri)
            : Guard.AgainstNull(_serviceBusConfiguration.Outbox!.WorkQueue);

        await using (var stream = await Guard.AgainstNull(state.GetTransportMessageStream()).CopyAsync().ConfigureAwait(false))
        {
            await queue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);
        }
    }
}