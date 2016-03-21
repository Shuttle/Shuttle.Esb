using System.Configuration;

namespace Shuttle.Esb
{
    public class TripleDESSection : ConfigurationSection
    {
        public static ServiceBusSection Open(string file)
        {
            return ConfigurationManager
                       .OpenMappedMachineConfiguration(new ConfigurationFileMap(file))
                       .GetSection("tripleDES") as ServiceBusSection;
        }

        [ConfigurationProperty("key", IsRequired = false, DefaultValue = null)]
        public string Key
        {
			get { return (string)this["key"]; }
        }
    }
}