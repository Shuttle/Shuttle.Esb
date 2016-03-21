using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class StartupPipeline : Pipeline
	{
		public StartupPipeline(IServiceBus bus)
		{
			Guard.AgainstNull(bus, "bus");

			State.Add(bus);

			RegisterStage("Initializing")
				.WithEvent<OnInitializing>()
				.WithEvent<OnInitializeQueueFactories>()
				.WithEvent<OnAfterInitializeQueueFactories>()
				.WithEvent<OnCreateQueues>()
				.WithEvent<OnAfterCreateQueues>()
				.WithEvent<OnInitializeMessageHandlerFactory>()
				.WithEvent<OnAfterInitializeMessageHandlerFactory>()
				.WithEvent<OnInitializeMessageRouteProvider>()
				.WithEvent<OnAfterInitializeMessageRouteProvider>()
				.WithEvent<OnInitializePipelineFactory>()
				.WithEvent<OnAfterInitializePipelineFactory>()
				.WithEvent<OnInitializeSubscriptionManager>()
				.WithEvent<OnAfterInitializeSubscriptionManager>()
				.WithEvent<OnInitializeIdempotenceService>()
				.WithEvent<OnAfterInitializeIdempotenceService>()
				.WithEvent<OnInitializeTransactionScopeFactory>()
				.WithEvent<OnAfterInitializeTransactionScopeFactory>();

			RegisterStage("Start")
				.WithEvent<OnStarting>()
				.WithEvent<OnStartInboxProcessing>()
				.WithEvent<OnAfterStartInboxProcessing>()
				.WithEvent<OnStartControlInboxProcessing>()
				.WithEvent<OnAfterStartControlInboxProcessing>()
				.WithEvent<OnStartOutboxProcessing>()
				.WithEvent<OnAfterStartOutboxProcessing>()
				.WithEvent<OnStartDeferredMessageProcessing>()
				.WithEvent<OnAfterStartDeferredMessageProcessing>()
				.WithEvent<OnStartWorker>()
				.WithEvent<OnAfterStartWorker>();

			RegisterStage("Final")
				.WithEvent<OnStarted>();

			RegisterObserver(new ServiceBusStartupObserver(bus));
		}
	}
}