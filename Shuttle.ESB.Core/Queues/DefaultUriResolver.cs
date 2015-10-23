using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
	public class DefaultUriResolver : IUriResolver
	{
		private readonly Dictionary<string, Uri> _uris = new Dictionary<string, Uri>();

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