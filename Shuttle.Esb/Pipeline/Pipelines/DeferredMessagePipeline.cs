using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class DeferredMessagePipeline : Pipeline
    {
        public DeferredMessagePipeline(IServiceBusConfiguration serviceBusConfiguration,
            IGetDeferredMessageObserver getDeferredMessageObserver,
            IDeserializeTransportMessageObserver deserializeTransportMessageObserver,
            IProcessDeferredMessageObserver processDeferredMessageObserver)
        {
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));

            State.SetWorkQueue(serviceBusConfiguration.Inbox.WorkQueue);
            State.SetErrorQueue(serviceBusConfiguration.Inbox.ErrorQueue);
            State.SetDeferredQueue(serviceBusConfiguration.Inbox.DeferredQueue);

            RegisterStage("Process")
                .WithEvent<OnGetMessage>()
                .WithEvent<OnAfterGetMessage>()
                .WithEvent<OnDeserializeTransportMessage>()
                .WithEvent<OnAfterDeserializeTransportMessage>()
                .WithEvent<OnProcessDeferredMessage>()
                .WithEvent<OnAfterProcessDeferredMessage>();

            RegisterObserver(Guard.AgainstNull(getDeferredMessageObserver, nameof(getDeferredMessageObserver)));
            RegisterObserver(Guard.AgainstNull(deserializeTransportMessageObserver, nameof(deserializeTransportMessageObserver)));
            RegisterObserver(Guard.AgainstNull(processDeferredMessageObserver, nameof(processDeferredMessageObserver)));
        }
    }
}