namespace Shuttle.Esb
{
	public interface IInboxQueueConfiguration : IWorkProcessorConfiguration
	{
		bool Distribute { get; set; }
		int DistributeSendCount { get; set; }
		bool HasDeferredQueue { get; }
		IQueue DeferredQueue { get; set; }
		string DeferredQueueUri { get; set; }
		DeferredMessageProcessor DeferredMessageProcessor { get; set; }
	}
}