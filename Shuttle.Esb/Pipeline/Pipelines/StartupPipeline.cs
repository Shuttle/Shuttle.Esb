using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class StartupPipeline : Pipeline
	{
		public StartupPipeline(IServiceBus bus)
		{
            Guard.AgainstNull(bus, "bus");

            State.SetServiceBus(bus);

            RegisterObserver(new ServiceBusStartupObserver(bus));

            RegisterStage("Initializing")
				.WithEvent<OnInitializing>()
				.WithEvent<OnCreateQueues>()
				.WithEvent<OnAfterCreateQueues>()
				.WithEvent<OnRegisterMessageHandlers>()
				.WithEvent<OnAfterInitializeMessageHandlerFactory>();

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
		}
    }
}