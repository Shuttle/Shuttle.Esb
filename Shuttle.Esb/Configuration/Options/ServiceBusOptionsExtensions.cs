using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public static class ServiceBusOptionsExtensions
    {
        public static bool HasInbox(this ServiceBusOptions options)
        {
            Guard.AgainstNull(options, nameof(options));

            return options.Inbox != null && !string.IsNullOrWhiteSpace(options.Inbox.WorkQueueUri);
        }

        public static bool HasDeferredQueue(this ServiceBusOptions options)
        {
            Guard.AgainstNull(options, nameof(options));

            return options.Inbox != null && !string.IsNullOrWhiteSpace(options.Inbox.DeferredQueueUri);
        }

        public static bool HasOutbox(this ServiceBusOptions options)
        {
            Guard.AgainstNull(options, nameof(options));

            return options.Outbox != null && !string.IsNullOrWhiteSpace(options.Outbox.WorkQueueUri);
        }
    }
}