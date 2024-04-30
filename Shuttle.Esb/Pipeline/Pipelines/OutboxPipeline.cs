using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class OutboxPipeline : Pipeline
    {
        public OutboxPipeline(IOptions<ServiceBusOptions> serviceBusOptions, IServiceBusConfiguration serviceBusConfiguration, IGetWorkMessageObserver getWorkMessageObserver,
            IDeserializeTransportMessageObserver deserializeTransportMessageObserver,
            ISendOutboxMessageObserver sendOutboxMessageObserver,
            IAcknowledgeMessageObserver acknowledgeMessageObserver, IOutboxExceptionObserver outboxExceptionObserver)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));

            if (!serviceBusConfiguration.HasOutbox())
            {
                return;
            }

            State.SetWorkQueue(serviceBusConfiguration.Outbox.WorkQueue);
            State.SetErrorQueue(serviceBusConfiguration.Outbox.ErrorQueue);

            State.SetDurationToIgnoreOnFailure(serviceBusOptions.Value.Outbox.DurationToIgnoreOnFailure);
            State.SetMaximumFailureCount(serviceBusOptions.Value.Outbox.MaximumFailureCount);

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

            RegisterObserver(Guard.AgainstNull(getWorkMessageObserver, nameof(getWorkMessageObserver)));
            RegisterObserver(Guard.AgainstNull(deserializeTransportMessageObserver, nameof(deserializeTransportMessageObserver)));
            RegisterObserver(Guard.AgainstNull(sendOutboxMessageObserver, nameof(sendOutboxMessageObserver)));
            RegisterObserver(Guard.AgainstNull(acknowledgeMessageObserver, nameof(acknowledgeMessageObserver)));
            
            RegisterObserver(Guard.AgainstNull(outboxExceptionObserver, nameof(outboxExceptionObserver))); // must be last
        }
    }
}