using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public interface IOutboxQueueConfiguration :
		IWorkQueueConfiguration,
		IErrorQueueConfiguration,
		IMessageFailureConfiguration,
		IThreadActivityConfiguration
	{
	}
}