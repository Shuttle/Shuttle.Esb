using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
    public interface IMessageHandlerInvoker
    {
        MessageHandlerInvokeResult Invoke(PipelineEvent pipelineEvent);
    }
}