using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public static class QueueExtensions
    {
        public static bool TryCreate(this IQueue queue)
        {
            return TryCreateAsync(queue, true).GetAwaiter().GetResult();
        }

        public static async ValueTask<bool> TryCreateAsync(this IQueue queue)
        {
            return await TryCreateAsync(queue, false).ConfigureAwait(false);
        }

        private static async ValueTask<bool> TryCreateAsync(this IQueue queue, bool sync)
        {
            var operation = queue as ICreateQueue;
            
            if (operation == null)
            {
                return false;
            }

            if (sync)
            {
                operation.Create();
            }
            else
            {
                await operation.CreateAsync().ConfigureAwait(false);
            }

            return true;
        }

        public static void Create(this IQueue queue)
        {
            CreateAsync(queue, true).GetAwaiter().GetResult();
        }

        public static async Task CreateAsync(this IQueue queue)
        {
            await CreateAsync(queue, false).ConfigureAwait(false);
        }

        private static async Task CreateAsync(this IQueue queue, bool sync)
        {
            Guard.AgainstNull(queue, nameof(queue));

            var operation = queue as ICreateQueue;

            if (operation == null)
            {
                throw new InvalidOperationException(string.Format(Resources.NotImplementedOnQueue,
                    queue.GetType().FullName, "ICreateQueue"));
            }

            if (sync)
            {
                operation.Create();
            }
            else
            {
                await operation.CreateAsync().ConfigureAwait(false);
            }
        }

        public static bool TryDrop(this IQueue queue)
        {
            return TryDropAsync(queue, true).GetAwaiter().GetResult();
        }

        public static async ValueTask<bool> TryDropAsync(this IQueue queue)
        {
            return await TryDropAsync(queue, false).ConfigureAwait(false);
        }

        private static async ValueTask<bool> TryDropAsync(this IQueue queue, bool sync)
        {
            var operation = queue as IDropQueue;

            if (operation == null)
            {
                return false;
            }

            if (sync)
            {
                operation.Drop();
            }
            else
            {
                await operation.DropAsync().ConfigureAwait(false);
            }

            return true;
        }

        public static void Drop(this IQueue queue)
        {
            DropAsync(queue, true).GetAwaiter().GetResult();
        }

        public static async Task DropAsync(this IQueue queue)
        {
            await DropAsync(queue, false).ConfigureAwait(false);
        }

        private static async Task DropAsync(this IQueue queue, bool sync)
        {
            Guard.AgainstNull(queue, nameof(queue));

            var operation = queue as IDropQueue;

            if (operation == null)
            {
                throw new InvalidOperationException(string.Format(Resources.NotImplementedOnQueue,
                    queue.GetType().FullName, "IDropQueue"));
            }

            await operation.DropAsync();
        }

        public static bool TryPurge(this IQueue queue)
        {
            return TryPurgeAsync(queue, true).GetAwaiter().GetResult();
        }

        public static async ValueTask<bool> TryPurgeAsync(this IQueue queue)
        {
            return await TryPurgeAsync(queue, false).ConfigureAwait(false);
        }

        private static async ValueTask<bool> TryPurgeAsync(this IQueue queue, bool sync)
        {
            var operation = queue as IPurgeQueue;

            if (operation == null)
            {
                return false;
            }

            await operation.PurgeAsync();

            return true;
        }

        public static void Purge(this IQueue queue)
        {
            PurgeAsync(queue, true).GetAwaiter().GetResult();
        }

        public static async Task PurgeAsync(this IQueue queue)
        {
            await PurgeAsync(queue, false).ConfigureAwait(false);
        }

        private static async Task PurgeAsync(this IQueue queue, bool sync)
        {
            Guard.AgainstNull(queue, nameof(queue));

            var operation = queue as IPurgeQueue;

            if (operation == null)
            {
                throw new InvalidOperationException(string.Format(Resources.NotImplementedOnQueue,
                    queue.GetType().FullName, "IPurgeQueue"));
            }

            await operation.PurgeAsync();
        }
    }
}