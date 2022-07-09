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

        public static bool HasControlInbox(this ServiceBusOptions options)
        {
            Guard.AgainstNull(options, nameof(options));

            return options.ControlInbox != null && !string.IsNullOrWhiteSpace(options.ControlInbox.WorkQueueUri);
        }

        public static bool HasOutbox(this ServiceBusOptions options)
        {
            Guard.AgainstNull(options, nameof(options));

            return options.Outbox != null && !string.IsNullOrWhiteSpace(options.Outbox.WorkQueueUri);
        }

        public static bool IsWorker(this ServiceBusOptions options)
        {
            Guard.AgainstNull(options, nameof(options));

            return options.Worker != null && !string.IsNullOrWhiteSpace(options.Worker.DistributorControlWorkQueueUri);
        }
    }
}