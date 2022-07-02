using System.Configuration;

namespace Shuttle.Esb
{
    public class WorkerElement : ConfigurationElement
    {
        [ConfigurationProperty("distributorControlUri", IsRequired = true)]
        public string DistributorControlUri => (string) this["distributorControlUri"];

        [ConfigurationProperty("threadAvailableNotificationIntervalSeconds", IsRequired = false, DefaultValue = 15)]
        public int ThreadAvailableNotificationIntervalSeconds => (int) this[
            "threadAvailableNotificationIntervalSeconds"];
    }
}