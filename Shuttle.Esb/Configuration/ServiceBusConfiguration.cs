using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ServiceBusConfiguration : IServiceBusConfiguration
    {
        private readonly IQueueService _queueService;

        public ServiceBusConfiguration(IQueueService queueService)
        {
            Guard.AgainstNull(queueService, nameof(queueService));

            _queueService = queueService;
        }

        public void Configure(ServiceBusOptions serviceBusOptions)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));

            if (serviceBusOptions.HasInbox())
            {
                Inbox = new InboxConfiguration
                {
                    WorkQueue = _queueService.Get(serviceBusOptions.Inbox.WorkQueueUri),
                    DeferredQueue =
                        string.IsNullOrWhiteSpace(serviceBusOptions.Inbox.DeferredQueueUri)
                            ? null
                            : _queueService.Get(serviceBusOptions.Inbox.DeferredQueueUri),
                    ErrorQueue =
                        string.IsNullOrWhiteSpace(serviceBusOptions.Inbox.ErrorQueueUri)
                            ? null
                            : _queueService.Get(serviceBusOptions.Inbox.ErrorQueueUri)
                };
            }

            if (serviceBusOptions.HasControlInbox())
            {
                ControlInbox = new ControlInboxConfiguration
                {
                    WorkQueue = _queueService.Get(serviceBusOptions.ControlInbox.WorkQueueUri),
                    ErrorQueue =
                        string.IsNullOrWhiteSpace(serviceBusOptions.ControlInbox.ErrorQueueUri)
                            ? null
                            : _queueService.Get(serviceBusOptions.ControlInbox.ErrorQueueUri)
                };
            }

            if (serviceBusOptions.HasOutbox())
            {
                Outbox = new OutboxConfiguration
                {
                    WorkQueue = _queueService.Get(serviceBusOptions.Outbox.WorkQueueUri),
                    ErrorQueue =
                        string.IsNullOrWhiteSpace(serviceBusOptions.Outbox.ErrorQueueUri)
                            ? null
                            : _queueService.Get(serviceBusOptions.Outbox.ErrorQueueUri)
                };
            }

            if (serviceBusOptions.IsWorker())
            {
                Worker = new WorkerConfiguration
                {
                    DistributorControlInboxWorkQueue =
                        _queueService.Get(serviceBusOptions.Worker.DistributorControlInboxWorkQueueUri)
                };
            }
        }

        public IInboxConfiguration Inbox { get; private set; }
        public IControlInboxConfiguration ControlInbox { get; private set; }
        public IOutboxConfiguration Outbox { get; private set; }
        public IWorkerConfiguration Worker { get; private set; }
    }
}