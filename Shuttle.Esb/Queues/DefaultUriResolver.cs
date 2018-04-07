using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class DefaultUriResolver : IUriResolver
    {
        private readonly Dictionary<string, Uri> _uris = new Dictionary<string, Uri>();

        public Uri GetTarget(Uri resolverUri)
        {
            var key = resolverUri.OriginalString.ToLower();

            _uris.TryGetValue(key, out var result);
            return result;
        }

        public void Add(Uri resolverUri, Uri targetUri)
        {
            Guard.AgainstNull(resolverUri, nameof(resolverUri));
            Guard.AgainstNull(targetUri, nameof(targetUri));

            _uris.Add(resolverUri.OriginalString.ToLower(), targetUri);
        }
    }
}