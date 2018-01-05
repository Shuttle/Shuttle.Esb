using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class ShutdownPipeline : Pipeline
    {
        public ShutdownPipeline(IEnumerable<IPipelineObserver> observers)
        {
            Guard.AgainstNull(observers, nameof(observers));

            var list = observers.ToList();

            RegisterObserver(list.Get<IShutdownProcessingObserver>());

            RegisterStage("Shutdown")
                .WithEvent<OnStopping>()
                .WithEvent<OnDisposeQueues>()
                .WithEvent<OnAfterDisposeQueues>();

            RegisterStage("Final")
                .WithEvent<OnStopped>();
        }
    }
}