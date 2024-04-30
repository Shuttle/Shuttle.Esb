using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class QueueService : IQueueService
    {
        private bool _disposed;
        private static readonly object Padlock = new object();
        private readonly IQueueFactoryService _queueFactoryService;

        private readonly List<IQueue> _queues = new List<IQueue>();
        private readonly IUriResolver _uriResolver;

        public QueueService(IQueueFactoryService queueFactoryService, IUriResolver uriResolver)
        {
            Guard.AgainstNull(queueFactoryService, nameof(queueFactoryService));
            Guard.AgainstNull(uriResolver, nameof(uriResolver));

            _queueFactoryService = queueFactoryService;
            _uriResolver = uriResolver;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            foreach (var queue in _queues)
            {
                QueueDisposing.Invoke(this, new QueueEventArgs(queue));

                queue.TryDispose();

                QueueDisposed.Invoke(this, new QueueEventArgs(queue));
            }

            _queues.Clear();

            _disposed = true;
        }

        public event EventHandler<QueueEventArgs> QueueCreated = delegate
        {
        };

        public event EventHandler<QueueEventArgs> QueueDisposing = delegate
        {
        };

        public event EventHandler<QueueEventArgs> QueueDisposed = delegate
        {
        };

        public IQueue Get(Uri uri)
        {
            Guard.AgainstNull(uri, nameof(uri));

            var queue = Find(uri);

            if (queue != null)
            {
                return queue;
            }

            lock (Padlock)
            {
                queue =
                    _queues.Find(candidate => candidate.Uri.Uri.Equals(uri));

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
                        throw new KeyNotFoundException(string.Format(Resources.UriNameNotFoundException,
                            _uriResolver.GetType().FullName,
                            uri));
                    }

                    queue = new ResolvedQueue(CreateQueue(_queueFactoryService.Get(resolvedQueueUri.Scheme), resolvedQueueUri),
                        queueUri);
                }
                else
                {
                    queue = CreateQueue(_queueFactoryService.Get(queueUri.Scheme), queueUri);
                }

                _queues.Add(queue);

                return queue;
            }
        }

        public bool Contains(Uri uri)
        {
            return Find(uri) != null;
        }

        private IQueue CreateQueue(IQueueFactory queueFactory, Uri queueUri)
        {
            var result = queueFactory.Create(queueUri);

            Guard.AgainstNull(result,
                string.Format(Resources.QueueFactoryCreatedNullQueue, queueFactory.GetType().FullName, queueUri));

            QueueCreated.Invoke(this, new QueueEventArgs(result));

            return result;
        }

        public IQueue Find(Uri uri)
        {
            Guard.AgainstNull(uri, nameof(uri));

            return _queues.Find(candidate => candidate.Uri.Uri.Equals(uri));
        }

        public ValueTask DisposeAsync()
        {
            Dispose();

            return new ValueTask();
        }
    }
}