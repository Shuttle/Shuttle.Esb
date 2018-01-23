using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class DeferredMessagePipeline : Pipeline
    {
        public DeferredMessagePipeline(IServiceBusConfiguration configuration,
            IGetDeferredMessageObserver getDeferredMessageObserver,
            IDeserializeTransportMessageObserver deserializeTransportMessageObserver,
            IProcessDeferredMessageObserver processDeferredMessageObserver)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(getDeferredMessageObserver, nameof(getDeferredMessageObserver));
            Guard.AgainstNull(deserializeTransportMessageObserver, nameof(deserializeTransportMessageObserver));
            Guard.AgainstNull(processDeferredMessageObserver, nameof(processDeferredMessageObserver));

            State.SetWorkQueue(configuration.Inbox.WorkQueue);
            State.SetErrorQueue(configuration.Inbox.ErrorQueue);
            State.SetDeferredQueue(configuration.Inbox.DeferredQueue);

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