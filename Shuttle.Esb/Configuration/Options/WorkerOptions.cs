namespace Shuttle.Esb
{
    public class WorkerOptions
    {
        public string DistributorControlInboxWorkQueueUri { get; set; }
        public int ThreadAvailableNotificationIntervalSeconds { get; set; }
    }
}