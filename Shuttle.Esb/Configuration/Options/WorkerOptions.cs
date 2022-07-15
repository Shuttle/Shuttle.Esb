namespace Shuttle.Esb
{
    public class WorkerOptions
    {
        public string DistributorControlWorkQueueUri { get; set; }
        public int ThreadAvailableNotificationIntervalSeconds { get; set; }
    }
}