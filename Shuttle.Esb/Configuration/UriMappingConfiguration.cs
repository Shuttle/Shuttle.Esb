using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class UriMappingConfiguration
    {
        public UriMappingConfiguration(Uri sourceUri, Uri targetUri)
        {
            Guard.AgainstNull(sourceUri, nameof(sourceUri));
            Guard.AgainstNull(targetUri, nameof(targetUri));

            SourceUri = sourceUri;
            TargetUri = targetUri;
        }

        public Uri SourceUri { get; set; }
        public Uri TargetUri { get; set; }
    }
}