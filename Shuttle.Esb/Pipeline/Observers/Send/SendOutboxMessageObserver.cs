using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb;

public interface ISendOutboxMessageObserver : IPipelineObserver<OnDispatchTransportMessage>
{
}

public class SendOutboxMessageObserver : ISendOutboxMessageObserver
{
    private readonly IQueueService _queueService;

    public SendOutboxMessageObserver(IQueueService queueService)
    {
        _queueService = Guard.AgainstNull(queueService);
    }

    public async Task ExecuteAsync(IPipelineContext<OnDispatchTransportMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var transportMessage = Guard.AgainstNull(state.GetTransportMessage());
        var receivedMessage = Guard.AgainstNull(state.GetReceivedMessage());

        Guard.AgainstNullOrEmptyString(transportMessage.RecipientInboxWorkQueueUri);

        var queue = _queueService.Get(transportMessage.RecipientInboxWorkQueueUri);

        await using (var stream = await receivedMessage.Stream.CopyAsync().ConfigureAwait(false))
        {
            await queue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);
        }
    }
}