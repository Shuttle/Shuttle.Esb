using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class StartupPipeline : Pipeline
    {
        public StartupPipeline(IStartupConfigurationObserver startupConfigurationObserver,
            IStartupProcessingObserver startupProcessingObserver)
        {
            Guard.AgainstNull(startupConfigurationObserver, nameof(startupConfigurationObserver));
            Guard.AgainstNull(startupProcessingObserver, nameof(startupProcessingObserver));

            RegisterStage("Configuration")
                .WithEvent<OnInitializing>()
                .WithEvent<OnConfigureUriResolver>()
                .WithEvent<OnAfterConfigureUriResolver>()
                .WithEvent<OnConfigureBrokerEndpoints>()
                .WithEvent<OnAfterConfigureBrokerEndpoints>()
                .WithEvent<OnCreatePhysicalBrokerEndpoints>()
                .WithEvent<OnAfterCreatePhysicalBrokerEndpoints>()
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

            RegisterObserver(startupConfigurationObserver);
            RegisterObserver(startupProcessingObserver);
        }
    }
}