namespace Shuttle.Esb
{
    public interface IWorkerConfiguration
    {
        IBrokerEndpoint DistributorControlInboxWorkBrokerEndpoint { get; set; }
        string DistributorControlUri { get; }
        int ThreadAvailableNotificationIntervalSeconds { get; }
    }
}