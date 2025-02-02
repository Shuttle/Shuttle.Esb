using System.Threading.Tasks;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public interface IMessageHandlerInvoker
{
    ValueTask<bool> InvokeAsync(IPipelineContext<OnHandleMessage> pipelineContext);
}