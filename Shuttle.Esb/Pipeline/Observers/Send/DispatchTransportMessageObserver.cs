using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb
{
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
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));
            Guard.AgainstNull(queueService, nameof(queueService));
            Guard.AgainstNull(idempotenceService, nameof(idempotenceService));

            _serviceBusConfiguration = serviceBusConfiguration;
            _queueService = queueService;
            _idempotenceService = idempotenceService;
        }

        public void Execute(OnDispatchTransportMessage pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnDispatchTransportMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnDispatchTransportMessage pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var transportMessage = Guard.AgainstNull(state.GetTransportMessage(), StateKeys.TransportMessage);
            var transportMessageReceived = state.GetTransportMessageReceived();

            if (transportMessageReceived != null &&
                // if this following is not in parenthesis, the condition does not short-circuit
                (
                    sync
                        ? _idempotenceService.AddDeferredMessage(transportMessageReceived, transportMessage, state.GetTransportMessageStream())
                        : await _idempotenceService.AddDeferredMessageAsync(transportMessageReceived, transportMessage, state.GetTransportMessageStream())
                )
               )
            {
                return;
            }

            Guard.AgainstNullOrEmptyString(transportMessage.RecipientInboxWorkQueueUri, nameof(transportMessage.RecipientInboxWorkQueueUri));

            var queue = !_serviceBusConfiguration.HasOutbox()
                ? _queueService.Get(transportMessage.RecipientInboxWorkQueueUri)
                : _serviceBusConfiguration.Outbox.WorkQueue;

            if (sync)
            {
                using (var stream = state.GetTransportMessageStream().Copy())
                {
                    queue.Enqueue(transportMessage, stream);
                }
            }
            else
            {
                await using (var stream = await state.GetTransportMessageStream().CopyAsync().ConfigureAwait(false))
                {
                    await queue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);
                }
            }
        }
    }
}