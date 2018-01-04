using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class DeferredMessagePipeline : Pipeline
    {
        public DeferredMessagePipeline(IServiceBusConfiguration configuration,
            GetDeferredMessageObserver getDeferredMessageObserver,
            DeserializeTransportMessageObserver deserializeTransportMessageObserver,
            ProcessDeferredMessageObserver processDeferredMessageObserver)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

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