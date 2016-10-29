using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class InboxMessagePipeline : ReceiveMessagePipeline
	{
	    public override void Assign(IServiceBus dependency)
	    {
            Guard.AgainstNull(dependency, "dependency");

            State.SetWorkQueue(dependency.Configuration.Inbox.WorkQueue);
            State.SetDeferredQueue(dependency.Configuration.Inbox.DeferredQueue);
            State.SetErrorQueue(dependency.Configuration.Inbox.ErrorQueue);

            State.SetDurationToIgnoreOnFailure(dependency.Configuration.Inbox.DurationToIgnoreOnFailure);
            State.SetMaximumFailureCount(dependency.Configuration.Inbox.MaximumFailureCount);
        }
    }
}