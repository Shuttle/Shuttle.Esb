using System;
using System.Threading.Tasks;

namespace Shuttle.Esb;

public static class QueueExtensions
{
    public static async Task CreateAsync(this IQueue queue)
    {
        if (queue is not ICreateQueue operation)
        {
            throw new InvalidOperationException(string.Format(Resources.NotImplementedOnQueue, queue.GetType().FullName, "ICreateQueue"));
        }

        await operation.CreateAsync().ConfigureAwait(false);
    }

    public static async Task DropAsync(this IQueue queue)
    {
        if (queue is not IDropQueue operation)
        {
            throw new InvalidOperationException(string.Format(Resources.NotImplementedOnQueue, queue.GetType().FullName, "IDropQueue"));
        }

        await operation.DropAsync();
    }

    public static async Task PurgeAsync(this IQueue queue)
    {
        if (queue is not IPurgeQueue operation)
        {
            throw new InvalidOperationException(string.Format(Resources.NotImplementedOnQueue, queue.GetType().FullName, "IPurgeQueue"));
        }

        await operation.PurgeAsync();
    }

    public static async ValueTask<bool> TryCreateAsync(this IQueue queue)
    {
        if (queue is not ICreateQueue operation)
        {
            return false;
        }

        await operation.CreateAsync().ConfigureAwait(false);

        return true;
    }

    public static async ValueTask<bool> TryDropAsync(this IQueue queue)
    {
        if (queue is not IDropQueue operation)
        {
            return false;
        }

        await operation.DropAsync().ConfigureAwait(false);

        return true;
    }

    public static async ValueTask<bool> TryPurgeAsync(this IQueue queue)
    {
        if (queue is not IPurgeQueue operation)
        {
            return false;
        }

        await operation.PurgeAsync();

        return true;
    }
}