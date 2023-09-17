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
        private readonly IServiceBusConfiguration _serviceBusConfiguration;
        private readonly IIdempotenceService _idempotenceService;
        private readonly IQueueService _queueService;

        public DispatchTransportMessageObserver(IServiceBusConfiguration serviceBusConfiguration, IQueueService queueService,
            IIdempotenceService idempotenceService)
        {
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));
            Guard.AgainstNull(queueService, nameof(queueService));
            Guard.AgainstNull(idempotenceService, nameof(idempotenceService));

            _serviceBusConfiguration = serviceBusConfiguration;
            _queueService = queueService;
            _idempotenceService = idempotenceService;
        }

        private async Task ExecuteAsync(OnDispatchTransportMessage pipelineEvent, bool sync)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();
            var transportMessageReceived = state.GetTransportMessageReceived();

            if (transportMessageReceived != null &&
                _idempotenceService.AddDeferredMessage(transportMessageReceived, transportMessage, state.GetTransportMessageStream()))
            {
                return;
            }

            Guard.AgainstNull(transportMessage, nameof(transportMessage));
            Guard.AgainstNullOrEmptyString(transportMessage.RecipientInboxWorkQueueUri, "uri");

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