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

        private async Task ExecuteAsync(OnDispatchTransportMessage pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var transportMessage = Guard.AgainstNull(state.GetTransportMessage(), StateKeys.TransportMessage);
            var receivedMessage = Guard.AgainstNull(state.GetReceivedMessage(), StateKeys.ReceivedMessage);

            Guard.AgainstNullOrEmptyString(transportMessage.RecipientInboxWorkQueueUri, nameof(transportMessage.RecipientInboxWorkQueueUri));

            var queue = _queueService.Get(transportMessage.RecipientInboxWorkQueueUri);

            if (sync)
            {
                using (var stream = receivedMessage.Stream.Copy())
                {
                    queue.Enqueue(transportMessage, stream);
                }
            }
            else
            {
                await using (var stream = await receivedMessage.Stream.CopyAsync().ConfigureAwait(false))
                {
                    await queue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);
                }
            }
        }

        public void Execute(OnDispatchTransportMessage pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnDispatchTransportMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }
    }
}