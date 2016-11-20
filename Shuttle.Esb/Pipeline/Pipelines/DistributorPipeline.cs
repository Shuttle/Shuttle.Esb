using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class DistributorPipeline : Pipeline
	{
		public DistributorPipeline(IServiceBus bus, GetWorkMessageObserver getWorkMessageObserver, DeserializeTransportMessageObserver deserializeTransportMessageObserver,
            DistributorMessageObserver distributorMessageObserver, SerializeTransportMessageObserver serializeTransportMessageObserver, 
            DispatchTransportMessageObserver dispatchTransportMessageObserver, AcknowledgeMessageObserver acknowledgeMessageObserver,
            DistributorExceptionObserver distributorExceptionObserver)
		{
            Guard.AgainstNull(bus, "bus");

            State.SetServiceBus(bus);

            State.SetWorkQueue(bus.Configuration.Inbox.WorkQueue);
            State.SetErrorQueue(bus.Configuration.Inbox.ErrorQueue);

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

			RegisterObserver(getWorkMessageObserver);
			RegisterObserver(deserializeTransportMessageObserver);
			RegisterObserver(distributorMessageObserver);
			RegisterObserver(serializeTransportMessageObserver);
			RegisterObserver(dispatchTransportMessageObserver);
			RegisterObserver(acknowledgeMessageObserver);

			RegisterObserver(distributorExceptionObserver); // must be last
		}
    }
}