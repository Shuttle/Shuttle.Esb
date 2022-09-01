namespace Shuttle.Esb
{
    public class WorkerConfiguration : IWorkerConfiguration
    {
        public IQueue DistributorControlInboxWorkQueue { get; set; }
    }
}