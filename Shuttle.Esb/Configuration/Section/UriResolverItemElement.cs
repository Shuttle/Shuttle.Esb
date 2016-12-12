using System.Configuration;

namespace Shuttle.Esb
{
	public class UriResolverItemElement : ConfigurationElement
	{
		[ConfigurationProperty("resolverUri", IsRequired = true)]
		public string ResolverUri
		{
			get { return (string) this["resolverUri"]; }
		}

		[ConfigurationProperty("targetUri", IsRequired = true)]
		public string TargetUri
		{
			get { return (string) this["targetUri"]; }
		}
	}
}