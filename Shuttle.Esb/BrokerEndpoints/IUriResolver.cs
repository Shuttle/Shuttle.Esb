using System;

namespace Shuttle.Esb
{
    public interface IUriResolver
    {
        Uri GetTarget(Uri resolverUri);
        void Add(Uri resolverUri, Uri targetUri);
    }
}