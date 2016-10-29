using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class DistributorPipeline : Pipeline, IDependency<IServiceBus>
	{
		public DistributorPipeline()
		{
			RegisterStage("Distribute")
				.WithEvent<OnGetMessage>()
				.WithEvent<OnDeserializeTransportMessage>()
				.WithEvent<OnAfterDeserializeTransportMessage>()
				.WithEvent<OnHandleDistributeMessage>()
				.WithEvent<OnAfterHandleDistributeMessage>()
				.WithEvent<OnSerializeTransportMessage>()
				.WithEvent<OnAfterSerializeTransportMessage>()
				.WithEvent<OnDispatchTransportMessage>()
				.WithEvent<OnAfterDispatchTransportMessage>()
				.WithEvent<OnAcknowledgeMessage>()
				.WithEvent<OnAfterAcknowledgeMessage>();

			RegisterObserver(new GetWorkMessageObserver());
			RegisterObserver(new DeserializeTransportMessageObserver());
			RegisterObserver(new DistributorMessageObserver());
			RegisterObserver(new SerializeTransportMessageObserver());
			RegisterObserver(new DispatchTransportMessageObserver());
			RegisterObserver(new AcknowledgeMessageObserver());

			RegisterObserver(new DistributorExceptionObserver()); // must be last
		}

	    public void Assign(IServiceBus dependency)
	    {
            Guard.AgainstNull(dependency, "dependency");

            State.SetWorkQueue(dependency.Configuration.Inbox.WorkQueue);
            State.SetErrorQueue(dependency.Configuration.Inbox.ErrorQueue);
        }
    }
}