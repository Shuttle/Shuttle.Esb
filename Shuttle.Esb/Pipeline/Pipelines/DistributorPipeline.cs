﻿using Shuttle.Core.Contract;
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
            Guard.AgainstNull(getWorkMessageObserver, nameof(getWorkMessageObserver));
            Guard.AgainstNull(deserializeTransportMessageObserver, nameof(deserializeTransportMessageObserver));
            Guard.AgainstNull(distributorMessageObserver, nameof(distributorMessageObserver));
            Guard.AgainstNull(serializeTransportMessageObserver, nameof(serializeTransportMessageObserver));
            Guard.AgainstNull(dispatchTransportMessageObserver, nameof(dispatchTransportMessageObserver));
            Guard.AgainstNull(acknowledgeMessageObserver, nameof(acknowledgeMessageObserver));
            Guard.AgainstNull(distributorExceptionObserver, nameof(distributorExceptionObserver));

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