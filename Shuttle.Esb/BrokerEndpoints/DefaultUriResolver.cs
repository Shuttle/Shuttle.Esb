using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class DefaultUriResolver : IUriResolver
    {
        private readonly Dictionary<string, Uri> _targetUris = new Dictionary<string, Uri>();

        public Uri GetTarget(Uri resolverUri)
        {
            if (!_targetUris.TryGetValue(resolverUri.OriginalString.ToLower(), out var result))
            {

            }

            return result;
        }

        public void Add(Uri resolverUri, Uri targetUri)
        {
            Guard.AgainstNull(resolverUri, nameof(resolverUri));
            Guard.AgainstNull(targetUri, nameof(targetUri));

            _targetUris.Add(resolverUri.OriginalString.ToLower(), targetUri);
        }
    }
}