namespace Shuttle.Esb
{
	public class WorkerConfiguration : IWorkerConfiguration
	{
		public IQueue DistributorControlInboxWorkQueue { get; set; }
		public string DistributorControlInboxWorkQueueUri { get; set; }
		public int ThreadAvailableNotificationIntervalSeconds { get; set; }
	}
}