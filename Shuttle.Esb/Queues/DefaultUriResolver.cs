using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class DefaultUriResolver : IUriResolver
	{
		private readonly Dictionary<string, Uri> _uris = new Dictionary<string, Uri>();

	    public DefaultUriResolver()
	    {
            if (ServiceBusConfiguration.ServiceBusSection == null ||
                ServiceBusConfiguration.ServiceBusSection.UriResolver == null)
            {
                return;
            }

            foreach (UriResolverItemElement uriRepositoryItemElement in ServiceBusConfiguration.ServiceBusSection.UriResolver)
            {
                Add(uriRepositoryItemElement.Name.ToLower(), new Uri(uriRepositoryItemElement.Uri));
            }
        }

        public Uri Get(string name)
		{
			var key = name.ToLower();

			return _uris.ContainsKey(key) ? _uris[key] : null;
		}

		public void Add(string sourceUri, string targetUri)
		{
			Guard.AgainstNullOrEmptyString(sourceUri, "sourceUri");
			Guard.AgainstNullOrEmptyString(targetUri, "targetUri");

			Add(sourceUri, new Uri(targetUri));
		}

		public void Add(string sourceUri, Uri targetUri)
		{
			Guard.AgainstNullOrEmptyString(sourceUri, "sourceUri");
			Guard.AgainstNull(targetUri, "targetUri");

			_uris.Add(sourceUri, targetUri);
		}
	}
}