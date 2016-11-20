using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class OutboxPipeline : Pipeline
    {
        public OutboxPipeline(IServiceBus bus, GetWorkMessageObserver getWorkMessageObserver, DeserializeTransportMessageObserver deserializeTransportMessageObserver,
            DeferTransportMessageObserver deferTransportMessageObserver, SendOutboxMessageObserver sendOutboxMessageObserver,
            AcknowledgeMessageObserver acknowledgeMessageObserver, OutboxExceptionObserver outboxExceptionObserver)
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

            RegisterObserver(getWorkMessageObserver);
            RegisterObserver(deserializeTransportMessageObserver);
            RegisterObserver(deferTransportMessageObserver);
            RegisterObserver(sendOutboxMessageObserver);

            RegisterObserver(acknowledgeMessageObserver);

            RegisterObserver(outboxExceptionObserver); // must be last
        }
    }
}