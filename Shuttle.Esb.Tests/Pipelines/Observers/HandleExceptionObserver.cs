using System.Threading.Tasks;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

public class HandleExceptionObserver : IPipelineObserver<OnPipelineException>
{
    public async Task ExecuteAsync(IPipelineContext<OnPipelineException> pipelineContext)
    {
        pipelineContext.Pipeline.MarkExceptionHandled();

        await Task.CompletedTask;
    }
}