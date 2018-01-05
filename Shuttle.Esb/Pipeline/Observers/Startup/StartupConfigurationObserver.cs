using System;
using Shuttle.Core.Container;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IStartupConfigurationObserver : 
        IPipelineObserver<OnConfigureUriResolver>, 
        IPipelineObserver<OnConfigureQueueManager>, 
        IPipelineObserver<OnConfigureQueues>, 
        IPipelineObserver<OnCreatePhysicalQueues>, 
        IPipelineObserver<OnConfigureMessageRouteProvider>
    {
    }

    public class StartupConfigurationObserver : IStartupConfigurationObserver
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IMessageRouteProvider _messageRouteProvider;
        private readonly IQueueManager _queueManager;
        private readonly IUriResolver _uriResolver;

        public StartupConfigurationObserver(IServiceBusConfiguration configuration, IQueueManager queueManager,
            IMessageRouteProvider messageRouteProvider, IUriResolver uriResolver)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(queueManager, nameof(queueManager));
            Guard.AgainstNull(messageRouteProvider, nameof(messageRouteProvider));
            Guard.AgainstNull(uriResolver, nameof(uriResolver));

            _queueManager = queueManager;
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

        public void Execute(OnConfigureQueueManager pipelineEvent)
        {
            foreach (var queueFactory in _configuration.Resolver.ResolveAll<IQueueFactory>())
            {
                _queueManager.RegisterQueueFactory(queueFactory);
            }
        }

        public void Execute(OnConfigureQueues pipelineEvent)
        {
            if (_configuration.HasControlInbox)
            {
                _configuration.ControlInbox.WorkQueue = _configuration.ControlInbox.WorkQueue ??
                                                        _queueManager.CreateQueue(
                                                            _configuration.ControlInbox.WorkQueueUri);

                _configuration.ControlInbox.ErrorQueue = _configuration.ControlInbox.ErrorQueue ??
                                                         _queueManager.CreateQueue(
                                                             _configuration.ControlInbox.ErrorQueueUri);
            }

            if (_configuration.HasInbox)
            {
                _configuration.Inbox.WorkQueue = _configuration.Inbox.WorkQueue ??
                                                 _queueManager.CreateQueue(_configuration.Inbox.WorkQueueUri);

                _configuration.Inbox.DeferredQueue = _configuration.Inbox.DeferredQueue ?? (
                                                         string.IsNullOrEmpty(_configuration.Inbox.DeferredQueueUri)
                                                             ? null
                                                             : _queueManager
                                                                 .CreateQueue(_configuration.Inbox.DeferredQueueUri));

                _configuration.Inbox.ErrorQueue = _configuration.Inbox.ErrorQueue ??
                                                  _queueManager.CreateQueue(_configuration.Inbox.ErrorQueueUri);
            }

            if (_configuration.HasOutbox)
            {
                _configuration.Outbox.WorkQueue = _configuration.Outbox.WorkQueue ??
                                                  _queueManager.CreateQueue(_configuration.Outbox.WorkQueueUri);

                _configuration.Outbox.ErrorQueue = _configuration.Outbox.ErrorQueue ??
                                                   _queueManager.CreateQueue(_configuration.Outbox.ErrorQueueUri);
            }

            if (_configuration.IsWorker)
            {
                _configuration.Worker.DistributorControlInboxWorkQueue =
                    _configuration.Worker.DistributorControlInboxWorkQueue ??
                    _queueManager.CreateQueue(_configuration.Worker.DistributorControlInboxWorkQueueUri);
            }
        }

        public void Execute(OnConfigureUriResolver pipelineEvent)
        {
            foreach (var configuration in _configuration.UriMapping)
            {
                _uriResolver.Add(configuration.SourceUri, configuration.TargetUri);
            }
        }

        public void Execute(OnCreatePhysicalQueues pipelineEvent)
        {
            if (!_configuration.CreateQueues)
            {
                return;
            }

            _queueManager.CreatePhysicalQueues(_configuration);
        }
    }
}