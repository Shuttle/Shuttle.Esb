using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class ServiceBusConfiguration : IServiceBusConfiguration
{
    public ServiceBusConfiguration(IOptions<ServiceBusOptions> serviceBusOptions, IQueueService queueService)
    {
        Guard.AgainstNull(queueService);
        
        var options = Guard.AgainstNull(Guard.AgainstNull(serviceBusOptions).Value);

        if (!string.IsNullOrWhiteSpace(options.Inbox.WorkQueueUri))
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

        if (!string.IsNullOrWhiteSpace(options.Outbox.WorkQueueUri))
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

    public IInboxConfiguration? Inbox { get; }
    public IOutboxConfiguration? Outbox { get; }
}