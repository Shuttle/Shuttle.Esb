﻿using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class OutboxPipeline : Pipeline
    {
        public OutboxPipeline(IOptions<ServiceBusOptions> options, IServiceBusConfiguration configuration, IGetWorkMessageObserver getWorkMessageObserver,
            IDeserializeTransportMessageObserver deserializeTransportMessageObserver,
            ISendOutboxMessageObserver sendOutboxMessageObserver,
            IAcknowledgeMessageObserver acknowledgeMessageObserver, IOutboxExceptionObserver outboxExceptionObserver)
        {
            Guard.AgainstNull(options, nameof(options));
            Guard.AgainstNull(options.Value, nameof(options.Value));
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(getWorkMessageObserver, nameof(getWorkMessageObserver));
            Guard.AgainstNull(deserializeTransportMessageObserver, nameof(deserializeTransportMessageObserver));
            Guard.AgainstNull(sendOutboxMessageObserver, nameof(sendOutboxMessageObserver));
            Guard.AgainstNull(acknowledgeMessageObserver, nameof(acknowledgeMessageObserver));
            Guard.AgainstNull(outboxExceptionObserver, nameof(outboxExceptionObserver));

            if (!configuration.HasOutbox())
            {
                return;
            }

            State.SetWorkQueue(configuration.Outbox.WorkQueue);
            State.SetErrorQueue(configuration.Outbox.ErrorQueue);

            State.SetDurationToIgnoreOnFailure(options.Value.Outbox.DurationToIgnoreOnFailure);
            State.SetMaximumFailureCount(options.Value.Outbox.MaximumFailureCount);

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
            RegisterObserver(sendOutboxMessageObserver);
            RegisterObserver(acknowledgeMessageObserver);
            RegisterObserver(outboxExceptionObserver); // must be last
        }
    }
}