using System;

namespace Shuttle.Esb
{
    public interface IQueueService : IDisposable, IAsyncDisposable
    {
        event EventHandler<QueueEventArgs> QueueCreated;
        event EventHandler<QueueEventArgs> QueueDisposing;
        event EventHandler<QueueEventArgs> QueueDisposed;

        IQueue Get(Uri uri);
        bool Contains(Uri uri);
    }
}