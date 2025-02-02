using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class UriMappingConfiguration
{
    public UriMappingConfiguration(Uri sourceUri, Uri targetUri)
    {
        SourceUri = Guard.AgainstNull(sourceUri);
        TargetUri = Guard.AgainstNull(targetUri);
    }

    public Uri SourceUri { get; set; }
    public Uri TargetUri { get; set; }
}