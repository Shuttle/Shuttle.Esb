using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class StartupConfigurationObserver :
        IPipelineObserver<OnConfigureUriResolver>,
        IPipelineObserver<OnConfigureQueueManager>,
        IPipelineObserver<OnCreateQueues>,
        IPipelineObserver<OnConfigureMessageRouteProvider>
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IQueueManager _queueManager;
        private readonly IMessageRouteProvider _messageRouteProvider;
        private readonly IUriResolver _uriResolver;

        public StartupConfigurationObserver(IServiceBusConfiguration configuration, IQueueManager queueManager, IMessageRouteProvider messageRouteProvider, IUriResolver uriResolver)
        {
            Guard.AgainstNull(configuration, "configuration");
            Guard.AgainstNull(queueManager, "queueManager");
            Guard.AgainstNull(messageRouteProvider, "messageRouteProvider");
            Guard.AgainstNull(uriResolver, "uriResolver");

            _queueManager = queueManager;
            _messageRouteProvider = messageRouteProvider;
            _uriResolver = uriResolver;
            _configuration = configuration;
        }

        public void Execute(OnCreateQueues pipelineEvent)
        {
            if (!_configuration.CreateQueues)
            {
                return;
            }

            _queueManager.CreatePhysicalQueues(_configuration);
        }

        public void Execute(OnConfigureQueueManager pipelineEvent)
        {
            foreach (var type in _configuration.QueueFactoryTypes)
            {
                _queueManager.RegisterQueueFactory(type);
            }

            if (_configuration.ScanForQueueFactories)
            {
                _queueManager.ScanForQueueFactories();
            }
        }

        public void Execute(OnConfigureMessageRouteProvider pipelineEvent)
        {
            var specificationFactory = new MessageRouteSpecificationFactory();

            foreach (var configuration in _configuration.MessageRoutes)
            {
                var messageRoute = _messageRouteProvider.Find(configuration.Uri);

                if (messageRoute == null)
                {
                    messageRoute = new MessageRoute(_queueManager.GetQueue(configuration.Uri));

                    _messageRouteProvider.Add(messageRoute);
                }

                foreach (var specification in configuration.Specifications)
                {
                    messageRoute.AddSpecification(specificationFactory.Create(specification.Name, specification.Value));
                }
            }
        }

        public void Execute(OnConfigureUriResolver pipelineEvent)
        {
            foreach (var configuration in _configuration.UriMapping)
            {
                _uriResolver.Add(configuration.SourceUri, configuration.TargetUri);
            }
        }
    }
}