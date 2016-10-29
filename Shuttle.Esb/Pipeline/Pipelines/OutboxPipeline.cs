using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class OutboxPipeline : Pipeline, IDependency<IServiceBus>
	{
		public OutboxPipeline()
		{
			RegisterStage("Read")
				.WithEvent<OnGetMessage>()
				.WithEvent<OnAfterGetMessage>()
				.WithEvent<OnDeserializeTransportMessage>()
				.WithEvent<OnAfterDeserializeTransportMessage>();

			RegisterStage("Send")
				.WithEvent<OnDispatchTransportMessage>()
				.WithEvent<OnAfterDispatchTransportMessage>()
				.WithEvent<OnAcknowledgeMessage>()
				.WithEvent<OnAfterAcknowledgeMessage>();

			RegisterObserver(new GetWorkMessageObserver());
			RegisterObserver(new DeserializeTransportMessageObserver());
			RegisterObserver(new DeferTransportMessageObserver());
			RegisterObserver(new SendOutboxMessageObserver());

			RegisterObserver(new AcknowledgeMessageObserver());

			RegisterObserver(new OutboxExceptionObserver()); // must be last
		}

	    public void Assign(IServiceBus dependency)
	    {
            Guard.AgainstNull(dependency, "dependency");

            State.SetWorkQueue(dependency.Configuration.Outbox.WorkQueue);
            State.SetErrorQueue(dependency.Configuration.Outbox.ErrorQueue);

            State.SetDurationToIgnoreOnFailure(dependency.Configuration.Outbox.DurationToIgnoreOnFailure);
            State.SetMaximumFailureCount(dependency.Configuration.Outbox.MaximumFailureCount);
        }
    }
}