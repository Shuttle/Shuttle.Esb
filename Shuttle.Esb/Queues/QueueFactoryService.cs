using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class QueueFactoryService : IQueueFactoryService, IDisposable
    {
        private readonly List<IQueueFactory> _queueFactories = new List<IQueueFactory>();

        public QueueFactoryService(IEnumerable<IQueueFactory> queueFactories = null)
        {
            _queueFactories.AddRange(queueFactories ?? Enumerable.Empty<IQueueFactory>());
        }

        public void Dispose()
        {
            foreach (var queueFactory in _queueFactories)
            {
                queueFactory.TryDispose();
            }

            _queueFactories.Clear();
        }

        public IQueueFactory Get(string scheme)
        {
            Guard.AgainstNullOrEmptyString(scheme, nameof(scheme));

            foreach (var factory in _queueFactories.Where(factory => factory.Scheme.Equals(scheme, StringComparison.InvariantCultureIgnoreCase)))
            {
                return factory;
            }

            throw new QueueFactoryNotFoundException(scheme);
        }

        public IEnumerable<IQueueFactory> Factories => _queueFactories.AsReadOnly();

        public void Register(IQueueFactory queueFactory)
        {
            Guard.AgainstNull(queueFactory, nameof(queueFactory));

            var factory = Get(queueFactory.Scheme);

            if (factory != null)
            {
                _queueFactories.Remove(factory);
            }

            _queueFactories.Add(queueFactory);
        }

        public bool Contains(string scheme)
        {
            return Get(scheme) != null;
        }
    }
}