using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class ShutdownPipeline : Pipeline
    {
        public ShutdownPipeline(IShutdownProcessingObserver shutdownProcessingObserver)
        {
            RegisterStage("Shutdown")
                .WithEvent<OnStopping>();

            RegisterStage("Final")
                .WithEvent<OnStopped>();

            RegisterObserver(Guard.AgainstNull(shutdownProcessingObserver, nameof(shutdownProcessingObserver)));
        }
    }
}