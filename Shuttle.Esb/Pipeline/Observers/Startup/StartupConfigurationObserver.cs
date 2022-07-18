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
        private readonly IQueueService _queueService;
        private readonly IServiceBusConfiguration _serviceBusConfiguration;
        private readonly ServiceBusOptions _serviceBusOptions;

        public StartupConfigurationObserver(IOptions<ServiceBusOptions> serviceBusOptions,
            IServiceBusConfiguration serviceBusConfiguration, IQueueService queueService)
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
                _serviceBusConfiguration.ControlInbox = new ControlInboxConfiguration
                {
                    WorkQueue = _serviceBusConfiguration.ControlInbox?.WorkQueue ??
                                _queueService.Create(_serviceBusOptions.ControlInbox.WorkQueueUri),
                    ErrorQueue = _serviceBusConfiguration.ControlInbox?.ErrorQueue ??
                                 (
                                     string.IsNullOrWhiteSpace(_serviceBusOptions.ControlInbox.ErrorQueueUri)
                                         ? null
                                         : _queueService.Create(_serviceBusOptions.ControlInbox.ErrorQueueUri)
                                 )
                };
            }

            if (_serviceBusOptions.HasInbox())
            {
                _serviceBusConfiguration.Inbox = new InboxConfiguration
                {
                    WorkQueue = _serviceBusConfiguration.Inbox?.WorkQueue ??
                                _queueService.Create(_serviceBusOptions.Inbox.WorkQueueUri),
                    DeferredQueue = _serviceBusConfiguration.Inbox?.DeferredQueue ??
                                    (
                                        string.IsNullOrWhiteSpace(_serviceBusOptions.Inbox.DeferredQueueUri)
                                            ? null
                                            : _queueService
                                                .Create(_serviceBusOptions.Inbox.DeferredQueueUri)
                                    ),
                    ErrorQueue = _serviceBusConfiguration.Inbox?.ErrorQueue ??
                                 (
                                     string.IsNullOrWhiteSpace(_serviceBusOptions.Inbox.ErrorQueueUri)
                                         ? null
                                         : _queueService.Create(_serviceBusOptions.Inbox.ErrorQueueUri)
                                 )
                };

                if (_serviceBusOptions.HasOutbox())
                {
                    _serviceBusConfiguration.Outbox = new OutboxConfiguration
                    {
                        WorkQueue = _serviceBusConfiguration.Outbox?.WorkQueue ??
                                    _queueService.Create(_serviceBusOptions.Outbox.WorkQueueUri),
                        ErrorQueue = _serviceBusConfiguration.Outbox?.ErrorQueue ??
                                     (
                                         string.IsNullOrWhiteSpace(_serviceBusOptions.Outbox.ErrorQueueUri)
                                             ? null
                                             : _queueService.Create(_serviceBusOptions.Outbox.ErrorQueueUri)
                                     )
                    };
                }

                if (_serviceBusOptions.IsWorker())
                {
                    _serviceBusConfiguration.Worker = new WorkerConfiguration
                    {
                        DistributorControlInboxWorkQueue =
                            _serviceBusConfiguration.Worker?.DistributorControlInboxWorkQueue ??
                            _queueService.Create(_serviceBusOptions.Worker.DistributorControlInboxWorkQueueUri)
                    };
                }
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