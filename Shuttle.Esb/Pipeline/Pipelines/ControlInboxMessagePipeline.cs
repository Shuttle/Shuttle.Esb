using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class ControlInboxMessagePipeline : ReceiveMessagePipeline
	{
	    public override void Assign(IServiceBus dependency)
	    {
            Guard.AgainstNull(dependency, "dependency");

            State.SetWorkQueue(dependency.Configuration.ControlInbox.WorkQueue);
            State.SetErrorQueue(dependency.Configuration.ControlInbox.ErrorQueue);
            State.SetDurationToIgnoreOnFailure(dependency.Configuration.ControlInbox.DurationToIgnoreOnFailure);
            State.SetMaximumFailureCount(dependency.Configuration.ControlInbox.MaximumFailureCount);
        }
    }
}