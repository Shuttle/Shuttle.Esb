using System.Threading.Tasks;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IMessageHandlerInvoker
    {
        MessageHandlerInvokeResult Invoke(OnHandleMessage pipelineEvent);
        Task<MessageHandlerInvokeResult> InvokeAsync(OnHandleMessage pipelineEvent);
    }
}