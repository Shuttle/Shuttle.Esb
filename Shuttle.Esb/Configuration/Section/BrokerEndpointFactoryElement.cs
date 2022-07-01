using System.Configuration;

namespace Shuttle.Esb
{
    public class BrokerEndpointFactoryElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type => (string) this["type"];
    }
}