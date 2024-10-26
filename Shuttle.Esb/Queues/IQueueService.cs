using System;

namespace Shuttle.Esb;

public interface IQueueService : IDisposable, IAsyncDisposable
{
    bool Contains(Uri uri);
    bool Contains(string uri);
    IQueue? Find(Uri uri);
    IQueue Get(Uri uri);
    IQueue Get(string uri);

    event EventHandler<QueueEventArgs> QueueCreated;
    event EventHandler<QueueEventArgs> QueueDisposed;
    event EventHandler<QueueEventArgs> QueueDisposing;
}