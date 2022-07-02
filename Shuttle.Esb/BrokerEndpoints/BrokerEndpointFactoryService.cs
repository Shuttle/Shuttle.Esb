using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class BrokerEndpointFactoryService : IBrokerEndpointFactoryService, IDisposable
    {
        private static readonly object Padlock = new object();

        private readonly ILog _log;
        private readonly List<IBrokerEndpointFactory> _brokerEndpointFactories = new List<IBrokerEndpointFactory>();

        public BrokerEndpointFactoryService(IEnumerable<IBrokerEndpointFactory> queueFactories = null)
        {
            _brokerEndpointFactories.AddRange(queueFactories ?? Enumerable.Empty<IBrokerEndpointFactory>());

            _log = Log.For(this);
        }

        public void Dispose()
        {
            foreach (var brokerEndpointFactory in _brokerEndpointFactories)
            {
                brokerEndpointFactory.AttemptDispose();
            }

            _brokerEndpointFactories.Clear();
        }

        public IBrokerEndpointFactory Get(string scheme)
        {
            return Uri.TryCreate(scheme, UriKind.Absolute, out var uri)
                ? Get(uri)
                : _brokerEndpointFactories.Find(
                    factory => factory.Scheme.Equals(scheme, StringComparison.InvariantCultureIgnoreCase));
        }

        public IBrokerEndpointFactory Get(Uri uri)
        {
            foreach (var factory in _brokerEndpointFactories.Where(factory => factory.CanCreate(uri)))
            {
                return factory;
            }

            throw new BrokerEndpointFactoryNotFoundException(uri.Scheme);
        }

        public IEnumerable<IBrokerEndpointFactory> Factories => _brokerEndpointFactories.AsReadOnly();

        public void Register(IBrokerEndpointFactory brokerEndpointFactory)
        {
            Guard.AgainstNull(brokerEndpointFactory, nameof(brokerEndpointFactory));

            var factory = Get(brokerEndpointFactory.Scheme);

            if (factory != null)
            {
                _brokerEndpointFactories.Remove(factory);

                _log.Warning(string.Format(Resources.DuplicateBrokerEndpointFactoryReplaced, brokerEndpointFactory.Scheme,
                    factory.GetType().FullName, brokerEndpointFactory.GetType().FullName));
            }

            _brokerEndpointFactories.Add(brokerEndpointFactory);

            if (Log.IsTraceEnabled)
            {
                _log.Trace(string.Format(Resources.BrokerEndpointFactoryRegistered, brokerEndpointFactory.Scheme,
                    brokerEndpointFactory.GetType().FullName));
            }
        }

        public bool Contains(string scheme)
        {
            return Get(scheme) != null;
        }
    }
}