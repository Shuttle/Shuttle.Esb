using System.Threading.Tasks;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IMessageHandlerInvoker
    {
        MessageHandlerInvokeResult Invoke(IPipelineEvent pipelineEvent);
        Task<MessageHandlerInvokeResult> InvokeAsync(IPipelineEvent pipelineEvent);
    }
}