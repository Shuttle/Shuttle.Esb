using System.Configuration;

namespace Shuttle.Esb
{
    public class WorkerElement : ConfigurationElement
    {
        [ConfigurationProperty("distributorControlWorkQueueUri", IsRequired = true)]
        public string DistributorControlWorkQueueUri => (string) this["distributorControlWorkQueueUri"];

        [ConfigurationProperty("threadAvailableNotificationIntervalSeconds", IsRequired = false, DefaultValue = 15)]
        public int ThreadAvailableNotificationIntervalSeconds => (int) this[
            "threadAvailableNotificationIntervalSeconds"];
    }
}