using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class StartupPipeline : Pipeline
    {
        public StartupPipeline(IEnumerable<IPipelineObserver> observers)
        {
            Guard.AgainstNull(observers, nameof(observers));

            var list = observers.ToList();

            RegisterObserver(list.Get<IStartupConfigurationObserver>());
            RegisterObserver(list.Get<IStartupProcessingObserver>());

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