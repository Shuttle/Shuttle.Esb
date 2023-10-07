using System;
using System.Threading.Tasks;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests.Pipelines.Observers;

public class ThrowExceptionObserver : IPipelineObserver<OnException>
{
    public void Execute(OnException pipelineEvent)
    {
        throw new Exception(string.Empty, new UnrecoverableHandlerException());
    }

    public Task ExecuteAsync(OnException pipelineEvent)
    {
        throw new Exception(string.Empty, new UnrecoverableHandlerException());
    }
}