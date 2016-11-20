using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class StartupPipeline : Pipeline
	{
		public StartupPipeline(StartupObserver startupObserver)
		{
            Guard.AgainstNull(startupObserver, "startupObserver");

            RegisterObserver(startupObserver);

            RegisterStage("Initializing")
				.WithEvent<OnInitializing>()
				.WithEvent<OnCreateQueues>()
				.WithEvent<OnAfterCreateQueues>();

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