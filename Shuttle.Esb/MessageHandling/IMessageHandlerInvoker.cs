using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public interface IMessageHandlerInvoker
	{
		MessageHandlerInvokeResult Invoke(IPipelineEvent pipelineEvent);
	}
}