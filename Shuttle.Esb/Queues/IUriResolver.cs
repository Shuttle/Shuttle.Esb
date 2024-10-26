using System;

namespace Shuttle.Esb;

public interface IUriResolver
{
    void Add(Uri sourceUri, Uri targetUri);
    Uri GetTarget(Uri sourceUri);
}