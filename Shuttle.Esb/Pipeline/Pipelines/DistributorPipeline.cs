using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class DistributorPipeline : Pipeline
    {
        public DistributorPipeline(IServiceBusConfiguration serviceBusConfiguration,
            IGetWorkMessageObserver getWorkMessageObserver,
            IDeserializeTransportMessageObserver deserializeTransportMessageObserver,
            IDistributorMessageObserver distributorMessageObserver,
            ISerializeTransportMessageObserver serializeTransportMessageObserver,
            IDispatchTransportMessageObserver dispatchTransportMessageObserver,
            IAcknowledgeMessageObserver acknowledgeMessageObserver,
            IDistributorExceptionObserver distributorExceptionObserver)
        {
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));

            State.SetWorkQueue(serviceBusConfiguration.Inbox.WorkQueue);
            State.SetErrorQueue(serviceBusConfiguration.Inbox.ErrorQueue);

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

            RegisterObserver(Guard.AgainstNull(getWorkMessageObserver, nameof(getWorkMessageObserver)));
            RegisterObserver(Guard.AgainstNull(deserializeTransportMessageObserver, nameof(deserializeTransportMessageObserver)));
            RegisterObserver(Guard.AgainstNull(distributorMessageObserver, nameof(distributorMessageObserver)));
            RegisterObserver(Guard.AgainstNull(serializeTransportMessageObserver, nameof(serializeTransportMessageObserver)));
            RegisterObserver(Guard.AgainstNull(dispatchTransportMessageObserver, nameof(dispatchTransportMessageObserver)));
            RegisterObserver(Guard.AgainstNull(acknowledgeMessageObserver, nameof(acknowledgeMessageObserver)));
            RegisterObserver(Guard.AgainstNull(distributorExceptionObserver, nameof(distributorExceptionObserver))); // must be last
        }
    }
}