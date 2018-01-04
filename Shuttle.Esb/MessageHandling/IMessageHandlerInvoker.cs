using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IMessageHandlerInvoker
    {
        MessageHandlerInvokeResult Invoke(IPipelineEvent pipelineEvent);
    }
}