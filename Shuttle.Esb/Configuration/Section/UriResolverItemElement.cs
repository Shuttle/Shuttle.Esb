using System.Configuration;

namespace Shuttle.Esb
{
    public class UriResolverItemElement : ConfigurationElement
    {
        [ConfigurationProperty("resolverUri", IsRequired = true)]
        public string ResolverUri => (string) this["resolverUri"];

        [ConfigurationProperty("targetUri", IsRequired = true)]
        public string TargetUri => (string) this["targetUri"];
    }
}