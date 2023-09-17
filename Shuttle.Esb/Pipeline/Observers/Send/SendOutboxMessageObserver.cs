using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb
{
    public interface ISendOutboxMessageObserver : IPipelineObserver<OnDispatchTransportMessage>
    {
    }

    public class SendOutboxMessageObserver : ISendOutboxMessageObserver
    {
        private readonly IQueueService _queueService;

        public SendOutboxMessageObserver(IQueueService queueService)
        {
            Guard.AgainstNull(queueService, nameof(queueService));

            _queueService = queueService;
        }

        public async Task Execute(OnDispatchTransportMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();
            var receivedMessage = state.GetReceivedMessage();

            Guard.AgainstNull(transportMessage, nameof(transportMessage));
            Guard.AgainstNull(receivedMessage, nameof(receivedMessage));
            Guard.AgainstNullOrEmptyString(transportMessage.RecipientInboxWorkQueueUri, nameof(transportMessage.RecipientInboxWorkQueueUri));

            var queue = _queueService.Get(transportMessage.RecipientInboxWorkQueueUri);

            await using (var stream = await receivedMessage.Stream.CopyAsync().ConfigureAwait(false))
            {
                await queue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);
            }
        }
    }
}