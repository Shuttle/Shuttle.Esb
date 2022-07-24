using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ServiceBusConfiguration : IServiceBusConfiguration
    {
        public ServiceBusConfiguration(IOptions<ServiceBusOptions> serviceBusOptions, IQueueService queueService)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions));
            Guard.AgainstNull(queueService, nameof(queueService));

            if (serviceBusOptions.Value.HasInbox())
            {
                Inbox = new InboxConfiguration
                {
                    WorkQueue = queueService.Get(serviceBusOptions.Value.Inbox.WorkQueueUri),
                    DeferredQueue =
                        string.IsNullOrWhiteSpace(serviceBusOptions.Value.Inbox.DeferredQueueUri)
                            ? null
                            : queueService.Get(serviceBusOptions.Value.Inbox.DeferredQueueUri),
                    ErrorQueue =
                        string.IsNullOrWhiteSpace(serviceBusOptions.Value.Inbox.ErrorQueueUri)
                            ? null
                            : queueService.Get(serviceBusOptions.Value.Inbox.ErrorQueueUri)
                };
            }

            if (serviceBusOptions.Value.HasControlInbox())
            {
                ControlInbox = new ControlInboxConfiguration
                {
                    WorkQueue = queueService.Get(serviceBusOptions.Value.ControlInbox.WorkQueueUri),
                    ErrorQueue =
                        string.IsNullOrWhiteSpace(serviceBusOptions.Value.ControlInbox.ErrorQueueUri)
                            ? null
                            : queueService.Get(serviceBusOptions.Value.ControlInbox.ErrorQueueUri)
                };
            }

            if (serviceBusOptions.Value.HasOutbox())
            {
                Outbox = new OutboxConfiguration
                {
                    WorkQueue = queueService.Get(serviceBusOptions.Value.Outbox.WorkQueueUri),
                    ErrorQueue =
                        string.IsNullOrWhiteSpace(serviceBusOptions.Value.Outbox.ErrorQueueUri)
                            ? null
                            : queueService.Get(serviceBusOptions.Value.Outbox.ErrorQueueUri)
                };
            }

            if (serviceBusOptions.Value.IsWorker())
            {
                Worker = new WorkerConfiguration
                {
                    DistributorControlInboxWorkQueue =
                        queueService.Get(serviceBusOptions.Value.Worker.DistributorControlInboxWorkQueueUri)
                };
            }
        }

        public IInboxConfiguration Inbox { get; }
        public IControlInboxConfiguration ControlInbox { get; }
        public IOutboxConfiguration Outbox { get; }
        public IWorkerConfiguration Worker { get; }
    }
}