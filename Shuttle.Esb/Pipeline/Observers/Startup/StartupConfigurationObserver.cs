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
        private readonly IServiceBusConfiguration _serviceBusConfiguration;
        private readonly IMessageRouteProvider _messageRouteProvider;
        private readonly ServiceBusOptions _serviceBusOptions;
        private readonly IQueueService _queueService;
        private readonly IUriResolver _uriResolver;

        public StartupConfigurationObserver(IOptions<ServiceBusOptions> serviceBusOptions, IServiceBusConfiguration serviceBusConfiguration, IQueueService queueService, IMessageRouteProvider messageRouteProvider, IUriResolver uriResolver)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));
            Guard.AgainstNull(queueService, nameof(queueService));
            Guard.AgainstNull(messageRouteProvider, nameof(messageRouteProvider));
            Guard.AgainstNull(uriResolver, nameof(uriResolver));

            _serviceBusOptions = serviceBusOptions.Value;
            _queueService = queueService;
            _messageRouteProvider = messageRouteProvider;
            _uriResolver = uriResolver;
            _serviceBusConfiguration = serviceBusConfiguration;
        }

        public void Execute(OnConfigureMessageRouteProvider pipelineEvent)
        {
            var specificationFactory = new MessageRouteSpecificationFactory();

            foreach (var configuration in _serviceBusConfiguration.MessageRoutes)
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
            if (_serviceBusOptions.HasControlInbox())
            {
                _serviceBusConfiguration.ControlInbox.WorkQueue = _serviceBusConfiguration.ControlInbox.WorkQueue ??
                                                        _queueService.Create(
                                                            _serviceBusOptions.ControlInbox.WorkQueueUri);

                _serviceBusConfiguration.ControlInbox.ErrorQueue = _serviceBusConfiguration.ControlInbox.ErrorQueue ?? (
                    string.IsNullOrWhiteSpace(_serviceBusOptions.ControlInbox.ErrorQueueUri)
                        ? null
                        : _queueService.Create(
                            _serviceBusOptions.ControlInbox.ErrorQueueUri)
                );
            }

            if (_serviceBusOptions.HasInbox())
            {
                _serviceBusConfiguration.Inbox.WorkQueue = _serviceBusConfiguration.Inbox.WorkQueue ??
                                                 _queueService.Create(_serviceBusOptions.Inbox.WorkQueueUri);

                _serviceBusConfiguration.Inbox.DeferredQueue = _serviceBusConfiguration.Inbox.DeferredQueue ?? (
                    string.IsNullOrWhiteSpace(_serviceBusOptions.Inbox.DeferredQueueUri)
                        ? null
                        : _queueService
                            .Create(_serviceBusOptions.Inbox.DeferredQueueUri)
                );

                _serviceBusConfiguration.Inbox.ErrorQueue = _serviceBusConfiguration.Inbox.ErrorQueue ?? (
                    string.IsNullOrWhiteSpace(_serviceBusOptions.Inbox.ErrorQueueUri)
                        ? null
                        : _queueService.Create(_serviceBusOptions.Inbox.ErrorQueueUri)
                );
            }

            if (_serviceBusOptions.HasOutbox())
            {
                _serviceBusConfiguration.Outbox.WorkQueue = _serviceBusConfiguration.Outbox.WorkQueue ??
                                                  _queueService.Create(_serviceBusOptions.Outbox.WorkQueueUri);

                _serviceBusConfiguration.Outbox.ErrorQueue = _serviceBusConfiguration.Outbox.ErrorQueue ?? (
                    string.IsNullOrWhiteSpace(_serviceBusOptions.Outbox.ErrorQueueUri)
                        ? null
                        : _queueService.Create(_serviceBusOptions.Outbox.ErrorQueueUri)
                );
            }

            if (_serviceBusOptions.IsWorker())
            {
                _serviceBusConfiguration.Worker.DistributorControlInboxWorkQueue =
                    _serviceBusConfiguration.Worker.DistributorControlInboxWorkQueue ??
                    _queueService.Create(_serviceBusConfiguration.Worker.DistributorControlInboxWorkQueueUri);
            }
        }

        public void Execute(OnConfigureUriResolver pipelineEvent)
        {
            foreach (var configuration in _serviceBusConfiguration.UriMapping)
            {
                _uriResolver.Add(configuration.SourceUri, configuration.TargetUri);
            }
        }

        public void Execute(OnCreatePhysicalQueues pipelineEvent)
        {
            if (!_serviceBusOptions.CreatePhysicalQueues)
            {
                return;
            }

            _queueService.CreatePhysicalQueues(_serviceBusConfiguration);
        }
    }
}