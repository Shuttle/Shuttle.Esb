namespace Shuttle.Esb
{
    public interface IWorkerConfiguration
    {
        IQueue DistributorControlInboxWorkQueue { get; set; }
    }
}