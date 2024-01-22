using System;
using System.Threading.Tasks;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

public class ThrowExceptionObserver : IPipelineObserver<OnException>
{
    public void Execute(OnException pipelineEvent)
    {
        throw new Exception(string.Empty, new UnrecoverableHandlerException());
    }

    public async Task ExecuteAsync(OnException pipelineEvent)
    {
        await Task.CompletedTask;

        throw new Exception(string.Empty, new UnrecoverableHandlerException());
    }
}