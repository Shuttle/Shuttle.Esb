using System;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransactionScope;

namespace Shuttle.Esb;

public class InboxMessagePipeline : Pipeline
{
    public InboxMessagePipeline(IServiceProvider serviceProvider, IOptions<ServiceBusOptions> serviceBusOptions, IServiceBusConfiguration serviceBusConfiguration, IGetWorkMessageObserver getWorkMessageObserver, IDeserializeTransportMessageObserver deserializeTransportMessageObserver, IDeferTransportMessageObserver deferTransportMessageObserver, IDeserializeMessageObserver deserializeMessageObserver, IDecryptMessageObserver decryptMessageObserver, IDecompressMessageObserver decompressMessageObserver, IHandleMessageObserver handleMessageObserver, IAcknowledgeMessageObserver acknowledgeMessageObserver, IReceiveExceptionObserver receiveExceptionObserver) 
        : base(serviceProvider)
    {
        AddStage("Read")
            .WithEvent<OnGetMessage>()
            .WithEvent<OnAfterGetMessage>()
            .WithEvent<OnDeserializeTransportMessage>()
            .WithEvent<OnAfterDeserializeTransportMessage>()
            .WithEvent<OnDecompressMessage>()
            .WithEvent<OnAfterDecompressMessage>()
            .WithEvent<OnDecryptMessage>()
            .WithEvent<OnAfterDecryptMessage>()
            .WithEvent<OnDeserializeMessage>()
            .WithEvent<OnAfterDeserializeMessage>();

        AddStage("Handle")
            .WithEvent<OnHandleMessage>()
            .WithEvent<OnAfterHandleMessage>()
            .WithEvent<OnCompleteTransactionScope>()
            .WithEvent<OnDisposeTransactionScope>()
            .WithEvent<OnAcknowledgeMessage>()
            .WithEvent<OnAfterAcknowledgeMessage>();

        AddObserver(Guard.AgainstNull(getWorkMessageObserver));
        AddObserver(Guard.AgainstNull(deserializeTransportMessageObserver));
        AddObserver(Guard.AgainstNull(deferTransportMessageObserver));
        AddObserver(Guard.AgainstNull(deserializeMessageObserver));
        AddObserver(Guard.AgainstNull(decryptMessageObserver));
        AddObserver(Guard.AgainstNull(decompressMessageObserver));
        AddObserver(Guard.AgainstNull(handleMessageObserver));
        AddObserver(Guard.AgainstNull(acknowledgeMessageObserver));

        AddObserver(Guard.AgainstNull(receiveExceptionObserver)); // must be last

        Guard.AgainstNull(Guard.AgainstNull(serviceBusOptions).Value);
        Guard.AgainstNull(serviceBusConfiguration);

        State.SetWorkQueue(Guard.AgainstNull(serviceBusConfiguration.Inbox!.WorkQueue));
        State.SetDeferredQueue(serviceBusConfiguration.Inbox.DeferredQueue);
        State.SetErrorQueue(serviceBusConfiguration.Inbox.ErrorQueue);

        State.SetDurationToIgnoreOnFailure(serviceBusOptions.Value.Inbox!.DurationToIgnoreOnFailure);
        State.SetMaximumFailureCount(serviceBusOptions.Value.Inbox.MaximumFailureCount);
    }
}