using System;
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

        public void Execute(OnDispatchTransportMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();
            var transportMessageReceived = state.GetTransportMessageReceived();

            if (transportMessageReceived != null)
            {
                if (_idempotenceService.AddDeferredMessage(transportMessageReceived, transportMessage,
                    state.GetTransportMessageStream()))
                {
                    return;
                }
            }

            Guard.AgainstNull(transportMessage, nameof(transportMessage));
            Guard.AgainstNullOrEmptyString(transportMessage.RecipientInboxWorkQueueUri, "uri");

            var queue = !_serviceBusConfiguration.HasOutbox()
                ? _queueService.Get(transportMessage.RecipientInboxWorkQueueUri)
                : _serviceBusConfiguration.Outbox.WorkQueue;

            using (var stream = state.GetTransportMessageStream().Copy())
            {
                queue.Enqueue(transportMessage, stream);
            }
        }
    }
}