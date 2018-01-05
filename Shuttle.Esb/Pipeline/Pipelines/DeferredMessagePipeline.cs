using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class DeferredMessagePipeline : Pipeline
    {
        public DeferredMessagePipeline(IServiceBusConfiguration configuration,
            IEnumerable<IPipelineObserver> observers)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(observers, nameof(observers));

            var list = observers.ToList();

            State.SetWorkQueue(configuration.Inbox.WorkQueue);
            State.SetErrorQueue(configuration.Inbox.ErrorQueue);
            State.SetDeferredQueue(configuration.Inbox.DeferredQueue);

            RegisterStage("Process")
                .WithEvent<OnGetMessage>()
                .WithEvent<OnDeserializeTransportMessage>()
                .WithEvent<OnAfterDeserializeTransportMessage>()
                .WithEvent<OnProcessDeferredMessage>()
                .WithEvent<OnAfterProcessDeferredMessage>();

            RegisterObserver(list.Get<IGetDeferredMessageObserver>());
            RegisterObserver(list.Get<IDeserializeTransportMessageObserver>());
            RegisterObserver(list.Get<IProcessDeferredMessageObserver>());
        }
    }
}