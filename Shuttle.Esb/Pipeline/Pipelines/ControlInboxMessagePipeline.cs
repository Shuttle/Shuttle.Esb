using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class ControlInboxMessagePipeline : ReceiveMessagePipeline
	{
	    public ControlInboxMessagePipeline(IServiceBus bus)
	    {
            Guard.AgainstNull(bus, "bus");

            State.SetServiceBus(bus);

            State.SetWorkQueue(bus.Configuration.ControlInbox.WorkQueue);
            State.SetErrorQueue(bus.Configuration.ControlInbox.ErrorQueue);
            State.SetDurationToIgnoreOnFailure(bus.Configuration.ControlInbox.DurationToIgnoreOnFailure);
            State.SetMaximumFailureCount(bus.Configuration.ControlInbox.MaximumFailureCount);
        }
    }
}