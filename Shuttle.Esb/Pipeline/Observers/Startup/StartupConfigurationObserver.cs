using System;
using Microsoft.Extensions.Options;
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
        private readonly ServiceBusOptions _options;
        private readonly IQueueService _queueService;
        private readonly IUriResolver _uriResolver;

        public StartupConfigurationObserver(IOptions<ServiceBusOptions> options, IServiceBusConfiguration configuration, IQueueService queueService, IMessageRouteProvider messageRouteProvider, IUriResolver uriResolver)
        {
            Guard.AgainstNull(options, nameof(options));
            Guard.AgainstNull(options.Value, nameof(options.Value));
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(queueService, nameof(queueService));
            Guard.AgainstNull(messageRouteProvider, nameof(messageRouteProvider));
            Guard.AgainstNull(uriResolver, nameof(uriResolver));

            _options = options.Value;
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
            if (_options.HasControlInbox())
            {
                _configuration.ControlInbox.WorkQueue = _configuration.ControlInbox.WorkQueue ??
                                                        _queueService.Create(
                                                            _options.ControlInbox.WorkQueueUri);

                _configuration.ControlInbox.ErrorQueue = _configuration.ControlInbox.ErrorQueue ?? (
                    string.IsNullOrWhiteSpace(_options.ControlInbox.ErrorQueueUri)
                        ? null
                        : _queueService.Create(
                            _options.ControlInbox.ErrorQueueUri)
                );
            }

            if (_options.HasInbox())
            {
                _configuration.Inbox.WorkQueue = _configuration.Inbox.WorkQueue ??
                                                 _queueService.Create(_options.Inbox.WorkQueueUri);

                _configuration.Inbox.DeferredQueue = _configuration.Inbox.DeferredQueue ?? (
                    string.IsNullOrWhiteSpace(_options.Inbox.DeferredQueueUri)
                        ? null
                        : _queueService
                            .Create(_options.Inbox.DeferredQueueUri)
                );

                _configuration.Inbox.ErrorQueue = _configuration.Inbox.ErrorQueue ?? (
                    string.IsNullOrWhiteSpace(_options.Inbox.ErrorQueueUri)
                        ? null
                        : _queueService.Create(_options.Inbox.ErrorQueueUri)
                );
            }

            if (_options.HasOutbox())
            {
                _configuration.Outbox.WorkQueue = _configuration.Outbox.WorkQueue ??
                                                  _queueService.Create(_options.Outbox.WorkQueueUri);

                _configuration.Outbox.ErrorQueue = _configuration.Outbox.ErrorQueue ?? (
                    string.IsNullOrWhiteSpace(_options.Outbox.ErrorQueueUri)
                        ? null
                        : _queueService.Create(_options.Outbox.ErrorQueueUri)
                );
            }

            if (_options.IsWorker())
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