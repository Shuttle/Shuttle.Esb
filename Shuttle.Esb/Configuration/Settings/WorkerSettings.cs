using System.Configuration;

namespace Shuttle.Esb
{
    public class WorkerSettings
    {
        public const string SectionName = "Shuttle:ServiceBus:Worker";

        public string DistributorControlWorkQueueUri { get; set; }
        public int ThreadAvailableNotificationIntervalSeconds { get; set; }
    }
}