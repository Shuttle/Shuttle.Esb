using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class DefaultUriResolver : IUriResolver
    {
        private readonly Dictionary<string, Uri> _targetUris = new Dictionary<string, Uri>();

        public DefaultUriResolver(IOptions<ServiceBusOptions> serviceBusOptions)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));

            foreach (var configuration in serviceBusOptions.Value.UriMappings)
            {
                Add(new Uri(configuration.SourceUri), new Uri(configuration.TargetUri));
            }
        }

        public Uri GetTarget(Uri sourceUri)
        {
            if (!_targetUris.TryGetValue(sourceUri.OriginalString.ToLower(), out var result))
            {

            }

            return result;
        }

        public void Add(Uri sourceUri, Uri targetUri)
        {
            Guard.AgainstNull(sourceUri, nameof(sourceUri));
            Guard.AgainstNull(targetUri, nameof(targetUri));

            _targetUris.Add(sourceUri.OriginalString.ToLower(), targetUri);
        }
    }
}