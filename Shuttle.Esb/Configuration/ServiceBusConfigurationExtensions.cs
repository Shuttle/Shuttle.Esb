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

        public static void CreatePhysicalQueues(this IServiceBusConfiguration serviceBusConfiguration)
        {
            if (serviceBusConfiguration.HasInbox())
            {
                CreateQueues(serviceBusConfiguration.Inbox);

                if (serviceBusConfiguration.Inbox.HasDeferredQueue())
                {
                    serviceBusConfiguration.Inbox.DeferredQueue.AttemptCreate();
                }
            }

            if (serviceBusConfiguration.HasOutbox())
            {
                CreateQueues(serviceBusConfiguration.Outbox);
            }

            if (serviceBusConfiguration.HasControlInbox())
            {
                CreateQueues(serviceBusConfiguration.ControlInbox);
            }
        }

        private static void CreateQueues(IWorkQueueConfiguration workQueueConfiguration)
        {
            workQueueConfiguration.WorkQueue.AttemptCreate();

            if (workQueueConfiguration is IErrorQueueConfiguration errorQueueConfiguration)
            {
                errorQueueConfiguration.ErrorQueue.AttemptCreate();
            }
        }
    }
}