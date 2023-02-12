using System.Threading.Tasks;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IMessageHandlerInvoker
    {
        Task<MessageHandlerInvokeResult> Invoke(IPipelineEvent pipelineEvent);
    }
}