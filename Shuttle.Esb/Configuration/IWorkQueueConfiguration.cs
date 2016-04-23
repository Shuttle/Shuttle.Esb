namespace Shuttle.Esb
{
	public interface IWorkQueueConfiguration : IThreadCount
	{
		IQueue WorkQueue { get; }
	}
}