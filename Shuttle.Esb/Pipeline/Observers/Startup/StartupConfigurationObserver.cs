using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IStartupConfigurationObserver : 
        IPipelineObserver<OnConfigureUriResolver>, 
        IPipelineObserver<OnConfigureBrokerEndpoints>, 
        IPipelineObserver<OnCreatePhysicalBrokerEndpoints>, 
        IPipelineObserver<OnConfigureMessageRouteProvider>
    {
    }

    public class StartupConfigurationObserver : IStartupConfigurationObserver
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IMessageRouteProvider _messageRouteProvider;
        private readonly IBrokerEndpointService _brokerEndpointService;
        private readonly IUriResolver _uriResolver;

        public StartupConfigurationObserver(IServiceBusConfiguration configuration, IBrokerEndpointService brokerEndpointService,
            IMessageRouteProvider messageRouteProvider, IUriResolver uriResolver)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(brokerEndpointService, nameof(brokerEndpointService));
            Guard.AgainstNull(messageRouteProvider, nameof(messageRouteProvider));
            Guard.AgainstNull(uriResolver, nameof(uriResolver));

            _brokerEndpointService = brokerEndpointService;
            _messageRouteProvider = messageRouteProvider;
            _uriResolver = uriResolver;
            _configuration = configuration;
        }

        public void Execute(OnConfigureMessageRouteProvider pipelineEvent)
        {
            var specificationFactory = new MessageRouteSpecificationFactory();

            foreach (var configuration in _configuration.MessageRoutes)
            {
                var messageRoute = _messageRouteProvider.Find(configuration.Uri);

                if (messageRoute == null)
                {
                    messageRoute = new MessageRoute(new Uri(configuration.Uri));

                    _messageRouteProvider.Add(messageRoute);
                }

                foreach (var specification in configuration.Specifications)
                {
                    messageRoute.AddSpecification(specificationFactory.Create(specification.Name, specification.Value));
                }
            }
        }

        public void Execute(OnConfigureBrokerEndpoints pipelineEvent)
        {
            if (_configuration.HasControl)
            {
                _configuration.Control.BrokerEndpoint = _configuration.Control.BrokerEndpoint ??
                                                        _brokerEndpointService.Create(
                                                            _configuration.Control.Uri);

                _configuration.Control.ErrorBrokerEndpoint = _configuration.Control.ErrorBrokerEndpoint ??
                                                         _brokerEndpointService.Create(
                                                             _configuration.Control.ErrorUri);
            }

            if (_configuration.HasInbox)
            {
                _configuration.Inbox.BrokerEndpoint = _configuration.Inbox.BrokerEndpoint ??
                                                 _brokerEndpointService.Create(_configuration.Inbox.Uri);

                _configuration.Inbox.DeferredBrokerEndpoint = _configuration.Inbox.DeferredBrokerEndpoint ?? (
                                                         string.IsNullOrEmpty(_configuration.Inbox.DeferredUri)
                                                             ? null
                                                             : _brokerEndpointService
                                                                 .Create(_configuration.Inbox.DeferredUri));

                _configuration.Inbox.ErrorBrokerEndpoint = _configuration.Inbox.ErrorBrokerEndpoint ??
                                                  _brokerEndpointService.Create(_configuration.Inbox.ErrorUri);
            }

            if (_configuration.HasOutbox)
            {
                _configuration.Outbox.BrokerEndpoint = _configuration.Outbox.BrokerEndpoint ??
                                                  _brokerEndpointService.Create(_configuration.Outbox.Uri);

                _configuration.Outbox.ErrorBrokerEndpoint = _configuration.Outbox.ErrorBrokerEndpoint ??
                                                   _brokerEndpointService.Create(_configuration.Outbox.ErrorUri);
            }

            if (_configuration.IsWorker)
            {
                _configuration.Worker.DistributorControlInboxWorkBrokerEndpoint =
                    _configuration.Worker.DistributorControlInboxWorkBrokerEndpoint ??
                    _brokerEndpointService.Create(_configuration.Worker.DistributorControlUri);
            }
        }

        public void Execute(OnConfigureUriResolver pipelineEvent)
        {
            foreach (var configuration in _configuration.UriMapping)
            {
                _uriResolver.Add(configuration.SourceUri, configuration.TargetUri);
            }
        }

        public void Execute(OnCreatePhysicalBrokerEndpoints pipelineEvent)
        {
            if (!_configuration.CreateBrokerEndpoints)
            {
                return;
            }

            _brokerEndpointService.CreatePhysical(_configuration);
        }
    }
}