using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class UriResolver : IUriResolver
{
    private readonly Dictionary<string, Uri> _targetUris = new();

    public UriResolver(IOptions<ServiceBusOptions> serviceBusOptions)
    {
        foreach (var configuration in Guard.AgainstNull(Guard.AgainstNull(serviceBusOptions).Value).UriMappings)
        {
            Add(new(configuration.SourceUri), new(configuration.TargetUri));
        }
    }

    public Uri GetTarget(Uri sourceUri)
    {
        if (!_targetUris.TryGetValue(sourceUri.OriginalString.ToLower(), out var result))
        {
            throw new InvalidOperationException(string.Format(Resources.CouldNotResolveSourceUriException, sourceUri.ToString()));
        }

        return result;
    }

    public void Add(Uri sourceUri, Uri targetUri)
    {
        _targetUris.Add(Guard.AgainstNull(sourceUri).OriginalString.ToLower(), targetUri);
    }
}