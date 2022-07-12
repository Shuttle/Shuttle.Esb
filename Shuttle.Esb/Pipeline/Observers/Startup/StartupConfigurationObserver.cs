using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IStartupConfigurationObserver :
        IPipelineObserver<OnConfigureQueues>,
        IPipelineObserver<OnCreatePhysicalQueues>
    {
    }

    public class StartupConfigurationObserver : IStartupConfigurationObserver
    {
        private readonly IServiceBusConfiguration _serviceBusConfiguration;
        private readonly ServiceBusOptions _serviceBusOptions;
        private readonly IQueueService _queueService;

        public StartupConfigurationObserver(IOptions<ServiceBusOptions> serviceBusOptions, IServiceBusConfiguration serviceBusConfiguration, IQueueService queueService)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));
            Guard.AgainstNull(queueService, nameof(queueService));

            _serviceBusOptions = serviceBusOptions.Value;
            _queueService = queueService;
            _serviceBusConfiguration = serviceBusConfiguration;
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