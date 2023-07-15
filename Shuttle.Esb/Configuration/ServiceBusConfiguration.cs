using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ServiceBusConfiguration : IServiceBusConfiguration
    {
        public ServiceBusConfiguration(IOptions<ServiceBusOptions> serviceBusOptions, IQueueService queueService)
        {
            Guard.AgainstNull(queueService, nameof(queueService));
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));

            var options = serviceBusOptions.Value;

            if (options.HasInbox())
            {
                Inbox = new InboxConfiguration
                {
                    WorkQueue = queueService.Get(options.Inbox.WorkQueueUri),
                    DeferredQueue =
                        string.IsNullOrWhiteSpace(options.Inbox.DeferredQueueUri)
                            ? null
                            : queueService.Get(options.Inbox.DeferredQueueUri),
                    ErrorQueue =
                        string.IsNullOrWhiteSpace(options.Inbox.ErrorQueueUri)
                            ? null
                            : queueService.Get(options.Inbox.ErrorQueueUri)
                };
            }

            if (options.HasControlInbox())
            {
                ControlInbox = new ControlInboxConfiguration
                {
                    WorkQueue = queueService.Get(options.ControlInbox.WorkQueueUri),
                    ErrorQueue =
                        string.IsNullOrWhiteSpace(options.ControlInbox.ErrorQueueUri)
                            ? null
                            : queueService.Get(options.ControlInbox.ErrorQueueUri)
                };
            }

            if (options.HasOutbox())
            {
                Outbox = new OutboxConfiguration
                {
                    WorkQueue = queueService.Get(options.Outbox.WorkQueueUri),
                    ErrorQueue =
                        string.IsNullOrWhiteSpace(options.Outbox.ErrorQueueUri)
                            ? null
                            : queueService.Get(options.Outbox.ErrorQueueUri)
                };
            }

            if (options.IsWorker())
            {
                Worker = new WorkerConfiguration
                {
                    DistributorControlInboxWorkQueue =
                        queueService.Get(options.Worker.DistributorControlInboxWorkQueueUri)
                };
            }
        }

        public void Configure(ServiceBusOptions serviceBusOptions)
        {
        }

        public IInboxConfiguration Inbox { get; }
        public IControlInboxConfiguration ControlInbox { get; }
        public IOutboxConfiguration Outbox { get; }
        public IWorkerConfiguration Worker { get; }
    }
}