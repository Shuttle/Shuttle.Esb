using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class StartupPipeline : Pipeline
    {
        public StartupPipeline(IStartupConfigurationObserver startupConfigurationObserver,
            IStartupProcessingObserver startupProcessingObserver, IStartupModulesObserver startupModulesObserver)
        {
            Guard.AgainstNull(startupConfigurationObserver, nameof(startupConfigurationObserver));
            Guard.AgainstNull(startupProcessingObserver, nameof(startupProcessingObserver));
            Guard.AgainstNull(startupModulesObserver, nameof(startupModulesObserver));

            RegisterStage("Configuration")
                .WithEvent<OnInitializing>()
                .WithEvent<OnConfigureQueues>()
                .WithEvent<OnAfterConfigureQueues>()
                .WithEvent<OnCreatePhysicalQueues>()
                .WithEvent<OnAfterCreatePhysicalQueues>();

            RegisterStage("Start")
                .WithEvent<OnStarting>()
                .WithEvent<OnStartInboxProcessing>()
                .WithEvent<OnAfterStartInboxProcessing>()
                .WithEvent<OnStartControlInboxProcessing>()
                .WithEvent<OnAfterStartControlInboxProcessing>()
                .WithEvent<OnStartOutboxProcessing>()
                .WithEvent<OnAfterStartOutboxProcessing>()
                .WithEvent<OnStartDeferredMessageProcessing>()
                .WithEvent<OnAfterStartDeferredMessageProcessing>();

            RegisterStage("Final")
                .WithEvent<OnStarted>();

            RegisterObserver(startupConfigurationObserver);
            RegisterObserver(startupProcessingObserver);
            RegisterObserver(startupModulesObserver);
        }
    }
}