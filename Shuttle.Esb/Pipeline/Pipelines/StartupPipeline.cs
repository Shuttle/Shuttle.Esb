using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class StartupPipeline : Pipeline
    {
        public StartupPipeline(StartupConfigurationObserver startupConfigurationObserver,
            StartupProcessingObserver startupProcessingObserver)
        {
            Guard.AgainstNull(startupConfigurationObserver, nameof(startupConfigurationObserver));
            Guard.AgainstNull(startupProcessingObserver, nameof(startupProcessingObserver));

            RegisterObserver(startupConfigurationObserver);
            RegisterObserver(startupProcessingObserver);

            RegisterStage("Configuration")
                .WithEvent<OnInitializing>()
                .WithEvent<OnConfigureUriResolver>()
                .WithEvent<OnAfterConfigureUriResolver>()
                .WithEvent<OnConfigureQueueManager>()
                .WithEvent<OnAfterConfigureQueueManager>()
                .WithEvent<OnConfigureQueues>()
                .WithEvent<OnAfterConfigureQueues>()
                .WithEvent<OnCreatePhysicalQueues>()
                .WithEvent<OnAfterCreatePhysicalQueues>()
                .WithEvent<OnConfigureMessageRouteProvider>()
                .WithEvent<OnAfterConfigureMessageRouteProvider>();

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
        }
    }
}