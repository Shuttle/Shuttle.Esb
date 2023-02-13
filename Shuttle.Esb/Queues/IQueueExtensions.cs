using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public static class QueueExtensions
    {
        public static bool CanCreate(this IQueueFactory factory, Uri uri)
        {
            return uri.Scheme.Equals(factory.Scheme, StringComparison.InvariantCultureIgnoreCase);
        }

        public static async ValueTask<bool> TryCreate(this IQueue queue)
        {
            var operation = queue as ICreateQueue;
            
            if (operation == null)
            {
                return false;
            }

            await operation.Create();

            return true;
        }

        public static async Task Create(this IQueue queue)
        {
            Guard.AgainstNull(queue, nameof(queue));

            var operation = queue as ICreateQueue;

            if (operation == null)
            {
                throw new InvalidOperationException(string.Format(Resources.NotImplementedOnQueue,
                    queue.GetType().FullName, "ICreateQueue"));
            }

            await operation.Create();
        }

        public static async ValueTask<bool> TryDrop(this IQueue queue)
        {
            var operation = queue as IDropQueue;

            if (operation == null)
            {
                return false;
            }

            await operation.Drop();

            return true;
        }

        public static async Task Drop(this IQueue queue)
        {
            Guard.AgainstNull(queue, nameof(queue));

            var operation = queue as IDropQueue;

            if (operation == null)
            {
                throw new InvalidOperationException(string.Format(Resources.NotImplementedOnQueue,
                    queue.GetType().FullName, "IDropQueue"));
            }

            await operation.Drop();
        }

        public static async ValueTask<bool> TryPurge(this IQueue queue)
        {
            var operation = queue as IPurgeQueue;

            if (operation == null)
            {
                return false;
            }

            await operation.Purge();

            return true;
        }

        public static async Task Purge(this IQueue queue)
        {
            Guard.AgainstNull(queue, nameof(queue));

            var operation = queue as IPurgeQueue;

            if (operation == null)
            {
                throw new InvalidOperationException(string.Format(Resources.NotImplementedOnQueue,
                    queue.GetType().FullName, "IPurgeQueue"));
            }

            await operation.Purge();
        }
    }
}