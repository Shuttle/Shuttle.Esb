using System;

namespace Shuttle.Esb
{
    public interface IQueueService
    {
        event EventHandler<QueueCreatedEventArgs> QueueCreated;

        IQueue Get(Uri uri);
        bool Contains(Uri uri);
    }
}