using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class DeferredMessagePipeline : Pipeline
	{
	    public DeferredMessagePipeline(IServiceBus bus, GetDeferredMessageObserver getDeferredMessageObserver, 
            DeserializeTransportMessageObserver deserializeTransportMessageObserver, ProcessDeferredMessageObserver processDeferredMessageObserver)
	    {
	        Guard.AgainstNull(bus, "bus");

            State.SetServiceBus(bus);

            State.SetWorkQueue(bus.Configuration.Inbox.WorkQueue);
            State.SetErrorQueue(bus.Configuration.Inbox.ErrorQueue);
            State.SetDeferredQueue(bus.Configuration.Inbox.DeferredQueue);

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