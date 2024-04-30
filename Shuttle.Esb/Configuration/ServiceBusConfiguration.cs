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
        }

        public void Configure(ServiceBusOptions serviceBusOptions)
        {
        }

        public IInboxConfiguration Inbox { get; }
        public IOutboxConfiguration Outbox { get; }
    }
}