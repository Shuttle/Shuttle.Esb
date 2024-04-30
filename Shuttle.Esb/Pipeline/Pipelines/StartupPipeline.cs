using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class StartupPipeline : Pipeline
    {
        public StartupPipeline(IStartupProcessingObserver startupProcessingObserver)
        {
            RegisterStage("Start")
                .WithEvent<OnStarting>()
                .WithEvent<OnCreatePhysicalQueues>()
                .WithEvent<OnAfterCreatePhysicalQueues>()
                .WithEvent<OnConfigureThreadPools>()
                .WithEvent<OnAfterConfigureThreadPools>()
                .WithEvent<OnStartThreadPools>()
                .WithEvent<OnAfterStartThreadPools>();

            RegisterStage("Final")
                .WithEvent<OnStarted>();

            RegisterObserver(Guard.AgainstNull(startupProcessingObserver, nameof(startupProcessingObserver)));
        }
    }
}