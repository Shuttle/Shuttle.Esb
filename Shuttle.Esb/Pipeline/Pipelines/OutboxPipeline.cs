using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class OutboxPipeline : Pipeline
    {
        public OutboxPipeline(IServiceBusConfiguration configuration, IEnumerable<IPipelineObserver> observers)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(observers, nameof(observers));

            var list = observers.ToList();

            State.SetWorkQueue(configuration.Outbox.WorkQueue);
            State.SetErrorQueue(configuration.Outbox.ErrorQueue);

            State.SetDurationToIgnoreOnFailure(configuration.Outbox.DurationToIgnoreOnFailure);
            State.SetMaximumFailureCount(configuration.Outbox.MaximumFailureCount);

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

            RegisterObserver(list.Get<IGetWorkMessageObserver>());
            RegisterObserver(list.Get<IDeserializeTransportMessageObserver>());
            RegisterObserver(list.Get<ISendOutboxMessageObserver>());
            RegisterObserver(list.Get<IAcknowledgeMessageObserver>());

            RegisterObserver(list.Get<IOutboxExceptionObserver>()); // must be last
        }
    }
}