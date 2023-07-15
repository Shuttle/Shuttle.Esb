using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class QueueService : IQueueService, IDisposable
    {
        private static readonly object Padlock = new object();

        private readonly List<IQueue> _queues = new List<IQueue>();
        private readonly IQueueFactoryService _queueFactoryService;
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
            foreach (var queue in _queues)
            {
                queue.TryDispose();
            }

            _queues.Clear();
        }

        public event EventHandler<QueueCreatedEventArgs> QueueCreated = delegate
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

        public IQueue Find(Uri uri)
        {
            Guard.AgainstNull(uri, nameof(uri));
            
            return _queues.Find(candidate => candidate.Uri.Uri.Equals(uri));
        }

        public IQueue Create(Uri uri)
        {
            return _queueFactoryService.Get(uri.Scheme).Create(uri);
        }

        private IQueue CreateQueue(IQueueFactory queueFactory, Uri queueUri)
        {
            var result = queueFactory.Create(queueUri);

            Guard.AgainstNull(result,
                string.Format(Resources.QueueFactoryCreatedNullQueue, queueFactory.GetType().FullName, queueUri));

            QueueCreated.Invoke(this, new QueueCreatedEventArgs(result));

            return result;
        }
    }
}