using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class InboxMessagePipeline : ReceiveMessagePipeline
	{
	    public InboxMessagePipeline(IServiceBus bus)
	    {
            Guard.AgainstNull(bus, "bus");

            State.SetServiceBus(bus);

            State.SetWorkQueue(bus.Configuration.Inbox.WorkQueue);
            State.SetDeferredQueue(bus.Configuration.Inbox.DeferredQueue);
            State.SetErrorQueue(bus.Configuration.Inbox.ErrorQueue);

            State.SetDurationToIgnoreOnFailure(bus.Configuration.Inbox.DurationToIgnoreOnFailure);
            State.SetMaximumFailureCount(bus.Configuration.Inbox.MaximumFailureCount);
        }
    }
}