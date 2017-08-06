using System.Configuration;

namespace Shuttle.Esb
{
    public class SpecificationElement : ConfigurationElement
    {
        [ConfigurationProperty("specification", IsRequired = true)]
        public string Name => (string) this["specification"];

        [ConfigurationProperty("value", IsRequired = true)]
        public string Value => (string) this["value"];
    }
}