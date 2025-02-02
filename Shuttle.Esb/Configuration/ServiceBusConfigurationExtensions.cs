using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public static class ServiceBusConfigurationExtensions
{
    public static async Task CreatePhysicalQueuesAsync(this IServiceBusConfiguration serviceBusConfiguration)
    {
        if (serviceBusConfiguration.HasInbox())
        {
            await CreateQueuesAsync(serviceBusConfiguration.Inbox!).ConfigureAwait(false);

            if (serviceBusConfiguration.Inbox!.HasDeferredQueue())
            {
                await serviceBusConfiguration.Inbox!.DeferredQueue!.TryCreateAsync().ConfigureAwait(false);
            }
        }

        if (serviceBusConfiguration.HasOutbox())
        {
            await CreateQueuesAsync(serviceBusConfiguration.Outbox!).ConfigureAwait(false);
        }
    }

    private static async Task CreateQueuesAsync(IWorkQueueConfiguration workQueueConfiguration)
    {
        if (workQueueConfiguration.WorkQueue != null)
        {
            await workQueueConfiguration.WorkQueue.TryCreateAsync().ConfigureAwait(false);
        }

        if (workQueueConfiguration is IErrorQueueConfiguration { ErrorQueue: not null } errorQueueConfiguration)
        {
            await errorQueueConfiguration.ErrorQueue.TryCreateAsync().ConfigureAwait(false);
        }
    }

    public static bool HasInbox(this IServiceBusConfiguration serviceBusConfiguration)
    {
        return Guard.AgainstNull(serviceBusConfiguration).Inbox != null;
    }

    public static bool HasOutbox(this IServiceBusConfiguration serviceBusConfiguration)
    {
        return Guard.AgainstNull(serviceBusConfiguration).Outbox != null;
    }
}