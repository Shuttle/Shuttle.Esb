using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class ShutdownPipeline : Pipeline
	{
		public ShutdownPipeline(ShutdownProcessingObserver shutdownProcessingObserver)
		{
            Guard.AgainstNull(shutdownProcessingObserver, "shutdownProcessingObserver");

            RegisterObserver(shutdownProcessingObserver);

			RegisterStage("Shutdown")
				.WithEvent<OnStopping>()
				.WithEvent<OnDisposeQueues>()
				.WithEvent<OnAfterDisposeQueues>();

			RegisterStage("Final")
				.WithEvent<OnStopped>();
		}
    }
}