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

        public IQueue Get(string uri)
        {
            Guard.AgainstNullOrEmptyString(uri, nameof(uri));

            var queue = FindQueue(uri);

            if (queue != null)
            {
                return queue;
            }

            lock (Padlock)
            {
                queue =
                    _queues.Find(
                        candidate => Find(candidate, uri));

                if (queue != null)
                {
                    return queue;
                }

                var queueUri = new Uri(uri);

                if (queueUri.Scheme.Equals("resolver"))
                {
                    var resolvedQueueUri = _uriResolver.GetTarget(queueUri);

                    if (resolvedQueueUri == null)
                    {
                        throw new KeyNotFoundException(string.Format(Resources.UriNameNotFoundException,
                            _uriResolver.GetType().FullName,
                            uri));
                    }

                    queue = new ResolvedQueue(CreateQueue(_queueFactoryService.Get(resolvedQueueUri), resolvedQueueUri),
                        queueUri);
                }
                else
                {
                    queue = CreateQueue(_queueFactoryService.Get(queueUri), queueUri);
                }

                _queues.Add(queue);

                return queue;
            }
        }

        public bool Contains(string uri)
        {
            return FindQueue(uri) != null;
        }

        public IQueue FindQueue(string uri)
        {
            Guard.AgainstNullOrEmptyString(uri, nameof(uri));
            
            return _queues.Find(
                candidate => Find(candidate, uri));
        }

        public IQueue Create(string uri)
        {
            return Create(new Uri(uri));
        }

        public IQueue Create(Uri uri)
        {
            return _queueFactoryService.Get(uri).Create(uri);
        }

        private IQueue CreateQueue(IQueueFactory queueFactory, Uri queueUri)
        {
            var result = queueFactory.Create(queueUri);

            Guard.AgainstNull(result,
                string.Format(Resources.QueueFactoryCreatedNullQueue, queueFactory.GetType().FullName, queueUri));

            QueueCreated.Invoke(this, new QueueCreatedEventArgs(result));

            return result;
        }

        private bool Find(IQueue candidate, string uri)
        {
            try
            {
                return candidate.Uri.ToString().Equals(uri, StringComparison.InvariantCultureIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}