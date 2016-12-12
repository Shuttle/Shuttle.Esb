using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class UriMappingConfiguration
    {
        public Uri SourceUri { get; set; }
        public Uri TargetUri { get; set; }

        public UriMappingConfiguration(Uri sourceUri, Uri targetUri)
        {
            Guard.AgainstNull(sourceUri, "sourceUri");
            Guard.AgainstNull(targetUri, "targetUri");

            SourceUri = sourceUri;
            TargetUri = targetUri;
        }
    }
}