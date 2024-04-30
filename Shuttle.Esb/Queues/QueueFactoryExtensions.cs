using System;

namespace Shuttle.Esb
{
    public static class QueueFactoryExtensions
    {
        public static bool CanCreate(this IQueueFactory factory, Uri uri)
        {
            return uri.Scheme.Equals(factory.Scheme, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}