using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb;

public class QueueService : IQueueService
{
    private static readonly object Padlock = new();
    private readonly IQueueFactoryService _queueFactoryService;

    private readonly List<IQueue> _queues = new();
    private readonly IUriResolver _uriResolver;
    private bool _disposed;

    public QueueService(IQueueFactoryService queueFactoryService, IUriResolver uriResolver)
    {
        _queueFactoryService = Guard.AgainstNull(queueFactoryService);
        _uriResolver = Guard.AgainstNull(uriResolver);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var queue in _queues)
        {
            QueueDisposing?.Invoke(this, new(queue));

            queue.TryDispose();

            QueueDisposed?.Invoke(this, new(queue));
        }

        _queues.Clear();

        _disposed = true;
    }

    public event EventHandler<QueueEventArgs>? QueueCreated;
    public event EventHandler<QueueEventArgs>? QueueDisposing;
    public event EventHandler<QueueEventArgs>? QueueDisposed;

    public bool Contains(Uri uri)
    {
        return Find(uri) != null;
    }

    public IQueue? Find(Uri uri)
    {
        Guard.AgainstNull(uri);

        return _queues.Find(candidate => candidate.Uri.Uri.Equals(uri));
    }

    public ValueTask DisposeAsync()
    {
        Dispose();

        return new();
    }

    public IQueue Get(Uri uri)
    {
        var queue = Find(Guard.AgainstNull(uri));

        if (queue != null)
        {
            return queue;
        }

        lock (Padlock)
        {
            queue = _queues.Find(candidate => candidate.Uri.Uri.Equals(uri));

            if (queue != null)
            {
                return queue;
            }

            var queueUri = uri;

            if (queueUri.Scheme.Equals("resolver"))
            {
                var resolvedQueueUri = _uriResolver.GetTarget(queueUri);

                if (resolvedQueueUri == null)
                {
                    throw new KeyNotFoundException(string.Format(Resources.UriNameNotFoundException, _uriResolver.GetType().FullName, uri));
                }

                queue = new ResolvedQueue(CreateQueue(_queueFactoryService.Get(resolvedQueueUri.Scheme), resolvedQueueUri), queueUri);
            }
            else
            {
                queue = CreateQueue(_queueFactoryService.Get(queueUri.Scheme), queueUri);
            }

            _queues.Add(queue);

            return queue;
        }
    }

    public bool Contains(string uri)
    {
        try
        {
            return Contains(new Uri(Guard.AgainstNullOrEmptyString(uri)));
        }
        catch (UriFormatException ex)
        {
            throw new UriFormatException($"{ex.Message} / uri = '{uri}'");
        }
    }

    public IQueue Get(string uri)
    {
        try
        {
            return Get(new Uri(Guard.AgainstNullOrEmptyString(uri)));
        }
        catch (UriFormatException ex)
        {
            throw new UriFormatException($"{ex.Message} / uri = '{uri}'");
        }
    }

    private IQueue CreateQueue(IQueueFactory queueFactory, Uri queueUri)
    {
        var result = queueFactory.Create(queueUri);

        Guard.AgainstNull(result, string.Format(Resources.QueueFactoryCreatedNullQueue, queueFactory.GetType().FullName, queueUri));

        QueueCreated?.Invoke(this, new(result));

        return result;
    }
}