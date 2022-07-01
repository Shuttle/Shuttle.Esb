using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb
{
    public interface IDispatchTransportMessageObserver : IPipelineObserver<OnDispatchTransportMessage>
    {
    }

    public class DispatchTransportMessageObserver : IDispatchTransportMessageObserver
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IIdempotenceService _idempotenceService;
        private readonly ILog _log;
        private readonly IBrokerEndpointService _brokerEndpointService;

        public DispatchTransportMessageObserver(IServiceBusConfiguration configuration, IBrokerEndpointService brokerEndpointService,
            IIdempotenceService idempotenceService)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(brokerEndpointService, nameof(brokerEndpointService));
            Guard.AgainstNull(idempotenceService, nameof(idempotenceService));

            _configuration = configuration;
            _brokerEndpointService = brokerEndpointService;
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
            Guard.AgainstNullOrEmptyString(transportMessage.RecipientUri, "uri");

            var queue = !_configuration.HasOutbox
                ? _brokerEndpointService.GetBrokerEndpoint(transportMessage.RecipientUri)
                : _configuration.Outbox.BrokerEndpoint;

            using (var stream = state.GetTransportMessageStream().Copy())
            {
                queue.Enqueue(transportMessage, stream);
            }
        }
    }
}