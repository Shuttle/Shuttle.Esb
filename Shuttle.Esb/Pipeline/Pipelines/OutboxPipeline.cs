using System;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public class OutboxPipeline : Pipeline
{
    public OutboxPipeline(IServiceProvider serviceProvider, IOptions<ServiceBusOptions> serviceBusOptions, IServiceBusConfiguration serviceBusConfiguration, IGetWorkMessageObserver getWorkMessageObserver, IDeserializeTransportMessageObserver deserializeTransportMessageObserver, ISendOutboxMessageObserver sendOutboxMessageObserver, IAcknowledgeMessageObserver acknowledgeMessageObserver, IOutboxExceptionObserver outboxExceptionObserver) 
        : base(serviceProvider)
    {
        Guard.AgainstNull(Guard.AgainstNull(serviceBusOptions).Value);

        if (serviceBusConfiguration.Outbox == null)
        {
            return;
        }

        State.SetWorkQueue(Guard.AgainstNull(serviceBusConfiguration.Outbox.WorkQueue));
        State.SetErrorQueue(serviceBusConfiguration.Outbox.ErrorQueue);

        State.SetDurationToIgnoreOnFailure(serviceBusOptions.Value.Outbox.DurationToIgnoreOnFailure);
        State.SetMaximumFailureCount(serviceBusOptions.Value.Outbox.MaximumFailureCount);

        AddStage("Read")
            .WithEvent<OnGetMessage>()
            .WithEvent<OnAfterGetMessage>()
            .WithEvent<OnDeserializeTransportMessage>()
            .WithEvent<OnAfterDeserializeTransportMessage>();

        AddStage("Send")
            .WithEvent<OnDispatchTransportMessage>()
            .WithEvent<OnAfterDispatchTransportMessage>()
            .WithEvent<OnAcknowledgeMessage>()
            .WithEvent<OnAfterAcknowledgeMessage>();

        AddObserver(Guard.AgainstNull(getWorkMessageObserver));
        AddObserver(Guard.AgainstNull(deserializeTransportMessageObserver));
        AddObserver(Guard.AgainstNull(sendOutboxMessageObserver));
        AddObserver(Guard.AgainstNull(acknowledgeMessageObserver));

        AddObserver(Guard.AgainstNull(outboxExceptionObserver)); // must be last
    }
}