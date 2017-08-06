namespace Shuttle.Esb
{
    public interface IWorkerConfiguration
    {
        IQueue DistributorControlInboxWorkQueue { get; set; }
        string DistributorControlInboxWorkQueueUri { get; }
        int ThreadAvailableNotificationIntervalSeconds { get; }
    }
}