using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class DistributorPipeline : Pipeline
    {
        public DistributorPipeline(IServiceBusConfiguration configuration,
            IEnumerable<IPipelineObserver> observers)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(observers, nameof(observers));

            var list = observers.ToList();

            State.SetWorkQueue(configuration.Inbox.WorkQueue);
            State.SetErrorQueue(configuration.Inbox.ErrorQueue);

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

            RegisterObserver(list.Get<IGetWorkMessageObserver>());
            RegisterObserver(list.Get<IDeserializeTransportMessageObserver>());
            RegisterObserver(list.Get<IDistributorMessageObserver>());
            RegisterObserver(list.Get<ISerializeTransportMessageObserver>());
            RegisterObserver(list.Get<IDispatchTransportMessageObserver>());
            RegisterObserver(list.Get<IAcknowledgeMessageObserver>());

            RegisterObserver(list.Get<IDistributorExceptionObserver>()); // must be last
        }
    }
}