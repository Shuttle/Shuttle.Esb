using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class OutboxPipeline : Pipeline
    {
        public OutboxPipeline(IServiceBus bus)
        {
            Guard.AgainstNull(bus, "bus");

            State.SetServiceBus(bus);

            State.SetWorkQueue(bus.Configuration.Outbox.WorkQueue);
            State.SetErrorQueue(bus.Configuration.Outbox.ErrorQueue);

            State.SetDurationToIgnoreOnFailure(bus.Configuration.Outbox.DurationToIgnoreOnFailure);
            State.SetMaximumFailureCount(bus.Configuration.Outbox.MaximumFailureCount);

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
    }
}