using System;

namespace Shuttle.Esb
{
    public interface IUriResolver
    {
        Uri GetTarget(Uri sourceUri);
        void Add(Uri sourceUri, Uri targetUri);
    }
}