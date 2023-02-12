using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public static class ServiceBusConfigurationExtensions
    {
        public static bool HasInbox(this IServiceBusConfiguration serviceBusConfiguration)
        {
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));

            return serviceBusConfiguration.Inbox != null;
        }

        public static bool HasControlInbox(this IServiceBusConfiguration serviceBusConfiguration)
        {
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));

            return serviceBusConfiguration.ControlInbox != null;
        }

        public static bool HasOutbox(this IServiceBusConfiguration serviceBusConfiguration)
        {
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));

            return serviceBusConfiguration.Outbox != null;
        }

        public static bool IsWorker(this IServiceBusConfiguration serviceBusConfiguration)
        {
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));

            return serviceBusConfiguration.Worker != null;
        }

        public static async Task CreatePhysicalQueues(this IServiceBusConfiguration serviceBusConfiguration)
        {
            if (serviceBusConfiguration.HasInbox())
            {
                await CreateQueues(serviceBusConfiguration.Inbox).ConfigureAwait(false);

                if (serviceBusConfiguration.Inbox.HasDeferredQueue())
                {
                    await serviceBusConfiguration.Inbox.DeferredQueue.TryCreate().ConfigureAwait(false);
                }
            }

            if (serviceBusConfiguration.HasOutbox())
            {
                await CreateQueues(serviceBusConfiguration.Outbox).ConfigureAwait(false);
            }

            if (serviceBusConfiguration.HasControlInbox())
            {
                await CreateQueues(serviceBusConfiguration.ControlInbox).ConfigureAwait(false);
            }
        }

        private static async Task CreateQueues(IWorkQueueConfiguration workQueueConfiguration)
        {
            await workQueueConfiguration.WorkQueue.TryCreate().ConfigureAwait(false);

            if (workQueueConfiguration is IErrorQueueConfiguration errorQueueConfiguration)
            {
                await errorQueueConfiguration.ErrorQueue.TryCreate().ConfigureAwait(false);
            }
        }
    }
}