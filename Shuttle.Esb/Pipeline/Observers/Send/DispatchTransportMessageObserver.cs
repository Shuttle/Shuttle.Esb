using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb
{
    public class DispatchTransportMessageObserver : IPipelineObserver<OnDispatchTransportMessage>
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IIdempotenceService _idempotenceService;
        private readonly ILog _log;
        private readonly IQueueManager _queueManager;

        public DispatchTransportMessageObserver(IServiceBusConfiguration configuration, IQueueManager queueManager,
            IIdempotenceService idempotenceService)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(queueManager, nameof(queueManager));
            Guard.AgainstNull(idempotenceService, nameof(idempotenceService));

            _configuration = configuration;
            _queueManager = queueManager;
            _idempotenceService = idempotenceService;
            _log = Log.For(this);
        }

        public void Execute(OnDispatchTransportMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();
            var transportMessageReceived = state.GetTransportMessageReceived();

            if (transportMessageReceived != null)
            {
                try
                {
                    if (_idempotenceService.AddDeferredMessage(transportMessageReceived, transportMessage,
                        state.GetTransportMessageStream()))
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _idempotenceService.AccessException(_log, ex, pipelineEvent.Pipeline);
                }
            }

            Guard.AgainstNull(transportMessage, nameof(transportMessage));
            Guard.AgainstNullOrEmptyString(transportMessage.RecipientInboxWorkQueueUri, "uri");

            var queue = !_configuration.HasOutbox
                ? _queueManager.GetQueue(transportMessage.RecipientInboxWorkQueueUri)
                : _configuration.Outbox.WorkQueue;

            using (var stream = state.GetTransportMessageStream().Copy())
            {
                queue.Enqueue(transportMessage, stream);
            }
        }
    }
}