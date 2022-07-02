using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class BrokerEndpointService : IBrokerEndpointService, IDisposable
    {
        private static readonly object Padlock = new object();

        private readonly ILog _log;
        private readonly List<IBrokerEndpoint> _brokerEndpoints = new List<IBrokerEndpoint>();
        private readonly IBrokerEndpointFactoryService _brokerEndpointFactoryService;
        private readonly IUriResolver _uriResolver;

        public BrokerEndpointService(IBrokerEndpointFactoryService brokerEndpointFactoryService, IUriResolver uriResolver)
        {
            Guard.AgainstNull(brokerEndpointFactoryService, nameof(brokerEndpointFactoryService));
            Guard.AgainstNull(uriResolver, nameof(uriResolver));

            _brokerEndpointFactoryService = brokerEndpointFactoryService;
            _uriResolver = uriResolver;

            _log = Log.For(this);
        }

        public void Dispose()
        {
            foreach (var queue in _brokerEndpoints)
            {
                queue.AttemptDispose();
            }

            _brokerEndpoints.Clear();
        }

        public IBrokerEndpoint Get(string uri)
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

                    var brokerEndpoint = CreateBrokerEndpoint(_brokerEndpointFactoryService.Get(resolvedUri), resolvedUri);

                    queue = brokerEndpoint.IsQueue() ? new ResolvedBrokerEndpointQueue((IQueue)brokerEndpoint, queueUri) : new ResolvedBrokerEndpoint(brokerEndpoint, queueUri);
                }
                else
                {
                    queue = CreateBrokerEndpoint(_brokerEndpointFactoryService.Get(queueUri), queueUri);
                }

                _brokerEndpoints.Add(queue);

                return queue;
            }
        }

        public bool Contains(string uri)
        {
            return FindBrokerEndpoint(uri) != null;
        }

        public IBrokerEndpoint FindBrokerEndpoint(string uri)
        {
            Guard.AgainstNullOrEmptyString(uri, nameof(uri));
            
            return _brokerEndpoints.Find(
                candidate => Find(candidate, uri));
        }

        public IBrokerEndpoint Create(string uri)
        {
            return Create(new Uri(uri));
        }

        public IBrokerEndpoint Create(Uri uri)
        {
            return _brokerEndpointFactoryService.Get(uri).Create(uri);
        }

        public void CreatePhysical(IServiceBusConfiguration configuration)
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