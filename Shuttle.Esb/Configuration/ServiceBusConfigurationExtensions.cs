﻿using System.Threading;
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

        public static bool HasOutbox(this IServiceBusConfiguration serviceBusConfiguration)
        {
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));

            return serviceBusConfiguration.Outbox != null;
        }

        public static async Task CreatePhysicalQueuesAsync(this IServiceBusConfiguration serviceBusConfiguration)
        {
            if (serviceBusConfiguration.HasInbox())
            {
                await CreateQueuesAsync(serviceBusConfiguration.Inbox).ConfigureAwait(false);

                if (serviceBusConfiguration.Inbox.HasDeferredQueue())
                {
                    await serviceBusConfiguration.Inbox.DeferredQueue.TryCreateAsync().ConfigureAwait(false);
                }
            }

            if (serviceBusConfiguration.HasOutbox())
            {
                await CreateQueuesAsync(serviceBusConfiguration.Outbox).ConfigureAwait(false);
            }
        }

        public static void CreatePhysicalQueues(this IServiceBusConfiguration serviceBusConfiguration)
        {
            if (serviceBusConfiguration.HasInbox())
            {
                CreateQueues(serviceBusConfiguration.Inbox);

                if (serviceBusConfiguration.Inbox.HasDeferredQueue())
                {
                    serviceBusConfiguration.Inbox.DeferredQueue.TryCreate();
                }
            }

            if (serviceBusConfiguration.HasOutbox())
            {
                CreateQueues(serviceBusConfiguration.Outbox);
            }
        }

        private static void CreateQueues(IWorkQueueConfiguration workQueueConfiguration)
        {
            workQueueConfiguration.WorkQueue.TryCreate();

            if (workQueueConfiguration is IErrorQueueConfiguration errorQueueConfiguration)
            {
                errorQueueConfiguration.ErrorQueue.TryCreate();
            }
        }

        private static async Task CreateQueuesAsync(IWorkQueueConfiguration workQueueConfiguration)
        {
            await workQueueConfiguration.WorkQueue.TryCreateAsync().ConfigureAwait(false);

            if (workQueueConfiguration is IErrorQueueConfiguration errorQueueConfiguration)
            {
                await errorQueueConfiguration.ErrorQueue.TryCreateAsync().ConfigureAwait(false);
            }
        }
    }
}