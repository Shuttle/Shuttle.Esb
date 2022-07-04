using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IStartupConfigurationObserver : 
        IPipelineObserver<OnConfigureUriResolver>, 
        IPipelineObserver<OnConfigureQueues>, 
        IPipelineObserver<OnCreatePhysicalQueues>, 
        IPipelineObserver<OnConfigureMessageRouteProvider>
    {
    }

    public class StartupConfigurationObserver : IStartupConfigurationObserver
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IMessageRouteProvider _messageRouteProvider;
        private readonly IQueueService _queueService;
        private readonly IUriResolver _uriResolver;

        public StartupConfigurationObserver(IServiceBusConfiguration configuration, IQueueService queueService,
            IMessageRouteProvider messageRouteProvider, IUriResolver uriResolver)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(queueService, nameof(queueService));
            Guard.AgainstNull(messageRouteProvider, nameof(messageRouteProvider));
            Guard.AgainstNull(uriResolver, nameof(uriResolver));

            _queueService = queueService;
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

        public void Execute(OnConfigureQueues pipelineEvent)
        {
            if (_configuration.HasControlInbox)
            {
                _configuration.ControlInbox.WorkQueue = _configuration.ControlInbox.WorkQueue ??
                                                        _queueService.Create(
                                                            _configuration.ControlInbox.WorkQueueUri);

                _configuration.ControlInbox.ErrorQueue = _configuration.ControlInbox.ErrorQueue ??
                                                         _queueService.Create(
                                                             _configuration.ControlInbox.ErrorQueueUri);
            }

            if (_configuration.HasInbox)
            {
                _configuration.Inbox.WorkQueue = _configuration.Inbox.WorkQueue ??
                                                 _queueService.Create(_configuration.Inbox.WorkQueueUri);

                _configuration.Inbox.DeferredQueue = _configuration.Inbox.DeferredQueue ?? (
                                                         string.IsNullOrEmpty(_configuration.Inbox.DeferredQueueUri)
                                                             ? null
                                                             : _queueService
                                                                 .Create(_configuration.Inbox.DeferredQueueUri));

                _configuration.Inbox.ErrorQueue = _configuration.Inbox.ErrorQueue ??
                                                  _queueService.Create(_configuration.Inbox.ErrorQueueUri);
            }

            if (_configuration.HasOutbox)
            {
                _configuration.Outbox.WorkQueue = _configuration.Outbox.WorkQueue ??
                                                  _queueService.Create(_configuration.Outbox.WorkQueueUri);

                _configuration.Outbox.ErrorQueue = _configuration.Outbox.ErrorQueue ??
                                                   _queueService.Create(_configuration.Outbox.ErrorQueueUri);
            }

            if (_configuration.IsWorker)
            {
                _configuration.Worker.DistributorControlInboxWorkQueue =
                    _configuration.Worker.DistributorControlInboxWorkQueue ??
                    _queueService.Create(_configuration.Worker.DistributorControlInboxWorkQueueUri);
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
            if (!_configuration.ShouldCreateQueues)
            {
                return;
            }

            _queueService.CreatePhysicalQueues(_configuration);
        }
    }
}