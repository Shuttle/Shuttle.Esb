using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public class ShutdownPipeline : Pipeline
{
    public ShutdownPipeline(IServiceProvider serviceProvider, IShutdownProcessingObserver shutdownProcessingObserver) 
        : base(serviceProvider)
    {
        RegisterStage("Shutdown")
            .WithEvent<OnStopping>();

        RegisterStage("Final")
            .WithEvent<OnStopped>();

        RegisterObserver(Guard.AgainstNull(shutdownProcessingObserver));
    }
}