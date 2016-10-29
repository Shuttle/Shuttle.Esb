using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class DeferredMessagePipeline : Pipeline, IDependency<IServiceBus>
	{
		public DeferredMessagePipeline()
		{
			RegisterStage("Process")
				.WithEvent<OnGetMessage>()
				.WithEvent<OnDeserializeTransportMessage>()
				.WithEvent<OnAfterDeserializeTransportMessage>()
				.WithEvent<OnProcessDeferredMessage>()
				.WithEvent<OnAfterProcessDeferredMessage>();

			RegisterObserver(new GetDeferredMessageObserver());
			RegisterObserver(new DeserializeTransportMessageObserver());
			RegisterObserver(new ProcessDeferredMessageObserver());
		}

	    public void Assign(IServiceBus dependency)
	    {
            Guard.AgainstNull(dependency, "dependency");

            State.SetWorkQueue(dependency.Configuration.Inbox.WorkQueue);
            State.SetErrorQueue(dependency.Configuration.Inbox.ErrorQueue);
            State.SetDeferredQueue(dependency.Configuration.Inbox.DeferredQueue);
        }
    }
}