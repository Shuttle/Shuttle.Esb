using System.Threading.Tasks;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

public class HandleExceptionObserver : IPipelineObserver<OnPipelineException>
{
    public void Execute(OnPipelineException pipelineEvent)
    {
        pipelineEvent.Pipeline.MarkExceptionHandled();
    }

    public async Task ExecuteAsync(OnPipelineException pipelineEvent)
    {
        pipelineEvent.Pipeline.MarkExceptionHandled();

        await Task.CompletedTask;
    }
}