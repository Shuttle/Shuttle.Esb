using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class BrokerEndpointService : IBrokerEndpointService, IDisposable
    {
        private static readonly object Padlock = new object();

        private readonly ILog _log;
        private readonly List<IBrokerEndpointFactory> _brokerEndpointFactories = new List<IBrokerEndpointFactory>();
        private readonly List<IBrokerEndpoint> _brokerEndpoints = new List<IBrokerEndpoint>();
        private readonly IUriResolver _uriResolver;

        public BrokerEndpointService(IUriResolver uriResolver, IEnumerable<IBrokerEndpointFactory> queueFactories = null)
        {
            Guard.AgainstNull(uriResolver, nameof(uriResolver));

            _uriResolver = uriResolver;

            _brokerEndpointFactories.AddRange(queueFactories ?? Enumerable.Empty<IBrokerEndpointFactory>());

            _log = Log.For(this);
        }

        public void Dispose()
        {
            foreach (var brokerEndpointFactory in _brokerEndpointFactories)
            {
                brokerEndpointFactory.AttemptDispose();
            }

            foreach (var queue in _brokerEndpoints)
            {
                queue.AttemptDispose();
            }

            _brokerEndpointFactories.Clear();
            _brokerEndpoints.Clear();
        }

        public IBrokerEndpointFactory GetBrokerEndpointFactory(string scheme)
        {
            return Uri.TryCreate(scheme, UriKind.Absolute, out var uri)
                ? GetBrokerEndpointFactory(uri)
                : _brokerEndpointFactories.Find(
                    factory => factory.Scheme.Equals(scheme, StringComparison.InvariantCultureIgnoreCase));
        }

        public IBrokerEndpointFactory GetBrokerEndpointFactory(Uri uri)
        {
            foreach (var factory in _brokerEndpointFactories.Where(factory => factory.CanCreate(uri)))
            {
                return factory;
            }

            throw new BrokerEndpointFactoryNotFoundException(uri.Scheme);
        }

        public IBrokerEndpoint GetBrokerEndpoint(string uri)
        {
            Guard.AgainstNullOrEmptyString(uri, "uri");

            var queue = FindBrokerEndpoint(uri);

            if (queue != null)
            {
                return queue;
            }

            lock (Padlock)
            {
                queue =
                    _brokerEndpoints.Find(
                        candidate => Find(candidate, uri));

                if (queue != null)
                {
                    return queue;
                }

                var queueUri = new Uri(uri);

                if (queueUri.Scheme.Equals("resolver"))
                {
                    var resolvedUri = _uriResolver.GetTarget(queueUri);

                    if (resolvedUri == null)
                    {
                        throw new KeyNotFoundException(string.Format(Resources.UriNameNotFoundException,
                            _uriResolver.GetType().FullName,
                            uri));
                    }

                    queue = new ResolvedBrokerEndpoint(CreateBrokerEndpoint(GetBrokerEndpointFactory(resolvedUri), resolvedUri),
                        queueUri);
                }
                else
                {
                    queue = CreateBrokerEndpoint(GetBrokerEndpointFactory(queueUri), queueUri);
                }

                _brokerEndpoints.Add(queue);

                return queue;
            }
        }

        public bool ContainsBrokerEndpoint(string uri)
        {
            return FindBrokerEndpoint(uri) != null;
        }

        public IBrokerEndpoint FindBrokerEndpoint(string uri)
        {
            Guard.AgainstNullOrEmptyString(uri, nameof(uri));
            
            return _brokerEndpoints.Find(
                candidate => Find(candidate, uri));
        }

        public IBrokerEndpoint CreateBrokerEndpoint(string uri)
        {
            return CreateBrokerEndpoint(new Uri(uri));
        }

        public IBrokerEndpoint CreateBrokerEndpoint(Uri uri)
        {
            return GetBrokerEndpointFactory(uri).Create(uri);
        }

        public IEnumerable<IBrokerEndpointFactory> BrokerEndpointFactories()
        {
            return new ReadOnlyCollection<IBrokerEndpointFactory>(_brokerEndpointFactories);
        }

        public void RegisterBrokerEndpointFactory(IBrokerEndpointFactory brokerEndpointFactory)
        {
            Guard.AgainstNull(brokerEndpointFactory, nameof(brokerEndpointFactory));

            var factory = GetBrokerEndpointFactory(brokerEndpointFactory.Scheme);

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

        public bool ContainsBrokerEndpointFactory(string scheme)
        {
            return GetBrokerEndpointFactory(scheme) != null;
        }

        public void CreatePhysicalBrokerEndpoint(IServiceBusConfiguration configuration)
        {
            if (configuration.HasInbox)
            {
                CreateBrokerEndpoints(configuration.Inbox);

                if (configuration.Inbox.HasDeferredBrokerEndpoint)
                {
                    configuration.Inbox.DeferredBrokerEndpoint.AttemptCreate();
                }
            }

            if (configuration.HasOutbox)
            {
                CreateBrokerEndpoints(configuration.Outbox);
            }

            if (configuration.HasControl)
            {
                CreateBrokerEndpoints(configuration.Control);
            }
        }

        private IBrokerEndpoint CreateBrokerEndpoint(IBrokerEndpointFactory brokerEndpointFactory, Uri queueUri)
        {
            var result = brokerEndpointFactory.Create(queueUri);

            Guard.AgainstNull(result,
                string.Format(Resources.BrokerEndpointFactoryCreatedNull, brokerEndpointFactory.GetType().FullName, queueUri));

            return result;
        }

        private bool Find(IBrokerEndpoint candidate, string uri)
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

                _log.Error(string.Format(Resources.FindBrokerEndpointException, candidateTypeName, candidateUri,
                    uri ?? "(comparison uri is null)", ex.Message));

                return false;
            }
        }

        private void CreateBrokerEndpoints(IWorkConfiguration workConfiguration)
        {
            workConfiguration.BrokerEndpoint.AttemptCreate();

            if (workConfiguration is IErrorConfiguration errorConfiguration)
            {
                errorConfiguration.ErrorBrokerEndpoint.AttemptCreate();
            }
        }
    }
}