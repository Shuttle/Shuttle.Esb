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
            Guard.AgainstNull(getDeferredMessageObserver, nameof(getDeferredMessageObserver));
            Guard.AgainstNull(deserializeTransportMessageObserver, nameof(deserializeTransportMessageObserver));
            Guard.AgainstNull(processDeferredMessageObserver, nameof(processDeferredMessageObserver));

            State.SetWorkQueue(serviceBusConfiguration.Inbox.WorkQueue);
            State.SetErrorQueue(serviceBusConfiguration.Inbox.ErrorQueue);
            State.SetDeferredQueue(serviceBusConfiguration.Inbox.DeferredQueue);

            RegisterStage("Process")
                .WithEvent<OnGetMessage>()
                .WithEvent<OnDeserializeTransportMessage>()
                .WithEvent<OnAfterDeserializeTransportMessage>()
                .WithEvent<OnProcessDeferredMessage>()
                .WithEvent<OnAfterProcessDeferredMessage>();

            RegisterObserver(getDeferredMessageObserver);
            RegisterObserver(deserializeTransportMessageObserver);
            RegisterObserver(processDeferredMessageObserver);
        }
    }
}