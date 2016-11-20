using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class SendDeferredObserver :
        IPipelineObserver<OnSendDeferred>,
        IPipelineObserver<OnAfterSendDeferred>
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly ISerializer _serializer;
        private readonly IIdempotenceService _idempotenceService;
        private readonly ILog _log;

        public SendDeferredObserver(IServiceBusConfiguration configuration, ISerializer serializer, IIdempotenceService idempotenceService)
        {
            Guard.AgainstNull(configuration, "configuration");
            Guard.AgainstNull(serializer, "serializer");
            Guard.AgainstNull(idempotenceService, "idempotenceService");

            _configuration = configuration;
            _serializer = serializer;
            _idempotenceService = idempotenceService;

            _log = Log.For(this);
        }

        public void Execute(OnSendDeferred pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            if (state.GetProcessingStatus() == ProcessingStatus.Ignore)
            {
                return;
            }

            var transportMessage = state.GetTransportMessage();

            try
            {
                foreach (var stream in _idempotenceService.GetDeferredMessages(transportMessage))
                {
                    var deferredTransportMessage =
                        (TransportMessage)_serializer.Deserialize(typeof(TransportMessage), stream);

                    state.GetServiceBus().Dispatch(deferredTransportMessage);

                    _idempotenceService.DeferredMessageSent(transportMessage, deferredTransportMessage);
                }
            }
            catch (Exception ex)
            {
                _idempotenceService.AccessException(_log, ex, pipelineEvent.Pipeline);
            }
        }

        public void Execute(OnAfterSendDeferred pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            if (pipelineEvent.Pipeline.Exception != null && !state.GetTransactionComplete())
            {
                return;
            }

            var bus = state.GetServiceBus();
            var transportMessage = state.GetTransportMessage();

            if (state.GetProcessingStatus() == ProcessingStatus.Ignore)
            {
                return;
            }

            try
            {
                _idempotenceService.ProcessingCompleted(transportMessage);
            }
            catch (Exception ex)
            {
                _idempotenceService.AccessException(_log, ex, pipelineEvent.Pipeline);
            }
        }
    }
}