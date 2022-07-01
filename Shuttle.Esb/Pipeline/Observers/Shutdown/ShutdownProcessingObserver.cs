using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public interface IShutdownProcessingObserver : 
        IPipelineObserver<OnStopping>, 
        IPipelineObserver<OnDisposeBrokerEndpoints>, 
        IPipelineObserver<OnStopped>
    {
    }

    public class ShutdownProcessingObserver : IShutdownProcessingObserver
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IServiceBusEvents _events;
        private readonly IBrokerEndpointService _brokerEndpointService;

        public ShutdownProcessingObserver(IServiceBusConfiguration configuration, IServiceBusEvents events,
            IBrokerEndpointService brokerEndpointService)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(events, nameof(events));

            _configuration = configuration;
            _events = events;
            _brokerEndpointService = brokerEndpointService;
        }

        public void Execute(OnDisposeBrokerEndpoints pipelineEvent)
        {
            if (_configuration.HasControl)
            {
                _configuration.Control.BrokerEndpoint.AttemptDispose();
                _configuration.Control.ErrorBrokerEndpoint.AttemptDispose();
            }

            if (_configuration.HasInbox)
            {
                _configuration.Inbox.BrokerEndpoint.AttemptDispose();
                _configuration.Inbox.DeferredBrokerEndpoint.AttemptDispose();
                _configuration.Inbox.ErrorBrokerEndpoint.AttemptDispose();
            }

            if (_configuration.HasOutbox)
            {
                _configuration.Outbox.BrokerEndpoint.AttemptDispose();
                _configuration.Outbox.ErrorBrokerEndpoint.AttemptDispose();
            }

            if (_configuration.IsWorker)
            {
                _configuration.Worker.DistributorControlInboxWorkBrokerEndpoint.AttemptDispose();
            }

            _brokerEndpointService.AttemptDispose();
        }

        public void Execute(OnStopped pipelineEvent)
        {
            _events.OnStopped(this, new PipelineEventEventArgs(pipelineEvent));
        }

        public void Execute(OnStopping pipelineEvent)
        {
            _events.OnStopping(this, new PipelineEventEventArgs(pipelineEvent));
        }
    }
}