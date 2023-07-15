using System;

namespace Shuttle.Esb
{
    public interface IQueueService
    {
        event EventHandler<QueueCreatedEventArgs> QueueCreated;

        IQueue Get(string uri);
        bool Contains(string uri);
    }
}