using System.Threading.Tasks;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

public class ThrowExceptionObserver : IPipelineObserver<OnException>
{
    public void Execute(OnException pipelineEvent)
    {
        throw new(string.Empty, new UnrecoverableHandlerException());
    }

    public async Task ExecuteAsync(IPipelineContext<OnException> pipelineContext)
    {
        await Task.CompletedTask;

        throw new(string.Empty, new UnrecoverableHandlerException());
    }
}