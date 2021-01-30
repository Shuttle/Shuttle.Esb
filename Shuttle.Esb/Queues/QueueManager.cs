using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class QueueManager : IQueueManager, IDisposable
    {
        private static readonly object Padlock = new object();

        private readonly ILog _log;
        private readonly List<IQueueFactory> _queueFactories = new List<IQueueFactory>();
        private readonly List<IQueue> _queues = new List<IQueue>();
        private readonly IUriResolver _uriResolver;

        public QueueManager(IUriResolver uriResolver)
        {
            Guard.AgainstNull(uriResolver, nameof(uriResolver));

            _uriResolver = uriResolver;

            _log = Log.For(this);
        }

        public void Dispose()
        {
            foreach (var queueFactory in _queueFactories)
            {
                queueFactory.AttemptDispose();
            }

            foreach (var queue in _queues)
            {
                queue.AttemptDispose();
            }

            _queueFactories.Clear();
            _queues.Clear();
        }

        public IQueueFactory GetQueueFactory(string scheme)
        {
            return Uri.TryCreate(scheme, UriKind.Absolute, out var uri)
                ? GetQueueFactory(uri)
                : _queueFactories.Find(
                    factory => factory.Scheme.Equals(scheme, StringComparison.InvariantCultureIgnoreCase));
        }

        public IQueueFactory GetQueueFactory(Uri uri)
        {
            foreach (var factory in _queueFactories.Where(factory => factory.CanCreate(uri)))
            {
                return factory;
            }

            throw new QueueFactoryNotFoundException(uri.Scheme);
        }

        public IQueue GetQueue(string uri)
        {
            Guard.AgainstNullOrEmptyString(uri, "uri");

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

                    queue = new ResolvedQueue(CreateQueue(GetQueueFactory(resolvedQueueUri), resolvedQueueUri),
                        queueUri);
                }
                else
                {
                    queue = CreateQueue(GetQueueFactory(queueUri), queueUri);
                }

                _queues.Add(queue);

                return queue;
            }
        }

        public bool ContainsQueue(string uri)
        {
            return FindQueue(uri) != null;
        }

        public IQueue FindQueue(string uri)
        {
            Guard.AgainstNullOrEmptyString(uri, nameof(uri));
            
            return _queues.Find(
                candidate => Find(candidate, uri));
        }

        public IQueue CreateQueue(string uri)
        {
            return CreateQueue(new Uri(uri));
        }

        public IQueue CreateQueue(Uri uri)
        {
            return GetQueueFactory(uri).Create(uri);
        }

        public IEnumerable<IQueueFactory> QueueFactories()
        {
            return new ReadOnlyCollection<IQueueFactory>(_queueFactories);
        }

        public void RegisterQueueFactory(IQueueFactory queueFactory)
        {
            Guard.AgainstNull(queueFactory, nameof(queueFactory));

            var factory = GetQueueFactory(queueFactory.Scheme);

            if (factory != null)
            {
                _queueFactories.Remove(factory);

                _log.Warning(string.Format(Resources.DuplicateQueueFactoryReplaced, queueFactory.Scheme,
                    factory.GetType().FullName, queueFactory.GetType().FullName));
            }

            _queueFactories.Add(queueFactory);

            if (Log.IsTraceEnabled)
            {
                _log.Trace(string.Format(Resources.QueueFactoryRegistered, queueFactory.Scheme,
                    queueFactory.GetType().FullName));
            }
        }

        public bool ContainsQueueFactory(string scheme)
        {
            return GetQueueFactory(scheme) != null;
        }

        public void CreatePhysicalQueues(IServiceBusConfiguration configuration)
        {
            if (configuration.HasInbox)
            {
                CreateQueues(configuration.Inbox);

                if (configuration.Inbox.HasDeferredQueue)
                {
                    configuration.Inbox.DeferredQueue.AttemptCreate();
                }
            }

            if (configuration.HasOutbox)
            {
                CreateQueues(configuration.Outbox);
            }

            if (configuration.HasControlInbox)
            {
                CreateQueues(configuration.ControlInbox);
            }
        }

        private IQueue CreateQueue(IQueueFactory queueFactory, Uri queueUri)
        {
            var result = queueFactory.Create(queueUri);

            Guard.AgainstNull(result,
                string.Format(Resources.QueueFactoryCreatedNullQueue, queueFactory.GetType().FullName, queueUri));

            return result;
        }

        private bool Find(IQueue candidate, string uri)
        {
            try
            {
                return candidate.Uri.ToString().Equals(uri, StringComparison.InvariantCultureIgnoreCase);
            }
            catch (Exception ex)
            {
                var candidateTypeName = "(candidate is null)";
                var candidateUri = "(candidate is null)";

                if (candidate != null)
                {
                    candidateTypeName = candidate.GetType().FullName;
                    candidateUri = candidate.Uri != null
                        ? candidate.Uri.ToString()
                        : "(candidate.Uri is null)";
                }

                _log.Error(string.Format(Resources.FindQueueException, candidateTypeName, candidateUri,
                    uri ?? "(comparison uri is null)", ex.Message));

                return false;
            }
        }

        private void CreateQueues(IWorkQueueConfiguration workQueueConfiguration)
        {
            workQueueConfiguration.WorkQueue.AttemptCreate();

            if (workQueueConfiguration is IErrorQueueConfiguration errorQueueConfiguration)
            {
                errorQueueConfiguration.ErrorQueue.AttemptCreate();
            }
        }
    }
}