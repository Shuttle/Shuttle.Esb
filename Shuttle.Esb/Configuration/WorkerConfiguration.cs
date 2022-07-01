namespace Shuttle.Esb
{
    public class WorkerConfiguration : IWorkerConfiguration
    {
        public IBrokerEndpoint DistributorControlInboxWorkBrokerEndpoint { get; set; }
        public string DistributorControlUri { get; set; }
        public int ThreadAvailableNotificationIntervalSeconds { get; set; }
    }
}