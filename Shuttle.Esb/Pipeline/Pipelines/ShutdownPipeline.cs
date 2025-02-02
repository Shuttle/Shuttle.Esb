using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public class ShutdownPipeline : Pipeline
{
    public ShutdownPipeline(IServiceProvider serviceProvider, IShutdownProcessingObserver shutdownProcessingObserver) 
        : base(serviceProvider)
    {
        AddStage("Shutdown")
            .WithEvent<OnStopping>();

        AddStage("Final")
            .WithEvent<OnStopped>();

        AddObserver(Guard.AgainstNull(shutdownProcessingObserver));
    }
}