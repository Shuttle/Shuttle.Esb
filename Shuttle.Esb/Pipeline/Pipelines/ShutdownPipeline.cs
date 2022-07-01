using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class ShutdownPipeline : Pipeline
    {
        public ShutdownPipeline(IShutdownProcessingObserver shutdownProcessingObserver)
        {
            Guard.AgainstNull(shutdownProcessingObserver, nameof(shutdownProcessingObserver));

            RegisterStage("Shutdown")
                .WithEvent<OnStopping>()
                .WithEvent<OnDisposeBrokerEndpoints>()
                .WithEvent<OnAfterDisposeBrokerEndpoints>();

            RegisterStage("Final")
                .WithEvent<OnStopped>();

            RegisterObserver(shutdownProcessingObserver);
        }
    }
}