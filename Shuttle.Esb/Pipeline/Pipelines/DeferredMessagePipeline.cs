using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public class DeferredMessagePipeline : Pipeline
{
    public DeferredMessagePipeline(IServiceBusConfiguration serviceBusConfiguration,
        IGetDeferredMessageObserver getDeferredMessageObserver,
        IDeserializeTransportMessageObserver deserializeTransportMessageObserver,
        IProcessDeferredMessageObserver processDeferredMessageObserver)
    {
        Guard.AgainstNull(serviceBusConfiguration);
        Guard.AgainstNull(serviceBusConfiguration.Inbox);

        State.SetWorkQueue(Guard.AgainstNull(serviceBusConfiguration.Inbox!.WorkQueue));
        State.SetErrorQueue(Guard.AgainstNull(serviceBusConfiguration.Inbox.ErrorQueue));
        State.SetDeferredQueue(Guard.AgainstNull(serviceBusConfiguration.Inbox.DeferredQueue));

        RegisterStage("Process")
            .WithEvent<OnGetMessage>()
            .WithEvent<OnAfterGetMessage>()
            .WithEvent<OnDeserializeTransportMessage>()
            .WithEvent<OnAfterDeserializeTransportMessage>()
            .WithEvent<OnProcessDeferredMessage>()
            .WithEvent<OnAfterProcessDeferredMessage>();

        RegisterObserver(Guard.AgainstNull(getDeferredMessageObserver));
        RegisterObserver(Guard.AgainstNull(deserializeTransportMessageObserver));
        RegisterObserver(Guard.AgainstNull(processDeferredMessageObserver));
    }
}