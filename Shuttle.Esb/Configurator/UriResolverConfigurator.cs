using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class UriResolverConfigurator : IConfigurator
    {
        public void Apply(IServiceBusConfiguration configuration)
        {
            if (ServiceBusSection.Get() == null ||
                ServiceBusSection.Get().UriResolver == null)
            {
                return;
            }

            foreach (UriResolverItemElement uriRepositoryItemElement in ServiceBusSection.Get().UriResolver)
            {
                configuration.AddUriMapping(Uri("ResolverUri", uriRepositoryItemElement.ResolverUri), Uri("TargetUri", uriRepositoryItemElement.TargetUri));
            }
        }

        private Uri Uri(string name, string uri)
        {
            try
            {
                return new Uri(uri);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format(EsbResources.MappingInvalidUriException, name, uri), ex);
            }
        }
    }
}