using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransactionScope;

namespace Shuttle.Esb;

public class InboxMessagePipeline : Pipeline
{
    public InboxMessagePipeline(IOptions<ServiceBusOptions> serviceBusOptions, IServiceBusConfiguration serviceBusConfiguration,
        IGetWorkMessageObserver getWorkMessageObserver,
        IDeserializeTransportMessageObserver deserializeTransportMessageObserver,
        IDeferTransportMessageObserver deferTransportMessageObserver,
        IDeserializeMessageObserver deserializeMessageObserver, IDecryptMessageObserver decryptMessageObserver,
        IDecompressMessageObserver decompressMessageObserver,
        IMessageHandlingSpecificationObserver messageHandlingSpecificationObserver, IIdempotenceObserver idempotenceObserver,
        IHandleMessageObserver handleMessageObserver, IAcknowledgeMessageObserver acknowledgeMessageObserver,
        ISendDeferredObserver sendDeferredObserver, IReceiveExceptionObserver receiveExceptionObserver)
    {
        RegisterStage("Read")
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

        RegisterStage("Handle")
            .WithEvent<OnEvaluateMessageHandling>()
            .WithEvent<OnAfterEvaluateMessageHandling>()
            .WithEvent<OnProcessIdempotenceMessage>()
            .WithEvent<OnHandleMessage>()
            .WithEvent<OnAfterHandleMessage>()
            .WithEvent<OnCompleteTransactionScope>()
            .WithEvent<OnDisposeTransactionScope>()
            .WithEvent<OnSendDeferred>()
            .WithEvent<OnAfterSendDeferred>()
            .WithEvent<OnAcknowledgeMessage>()
            .WithEvent<OnAfterAcknowledgeMessage>();

        RegisterObserver(Guard.AgainstNull(getWorkMessageObserver));
        RegisterObserver(Guard.AgainstNull(deserializeTransportMessageObserver));
        RegisterObserver(Guard.AgainstNull(deferTransportMessageObserver));
        RegisterObserver(Guard.AgainstNull(deserializeMessageObserver));
        RegisterObserver(Guard.AgainstNull(decryptMessageObserver));
        RegisterObserver(Guard.AgainstNull(decompressMessageObserver));
        RegisterObserver(Guard.AgainstNull(messageHandlingSpecificationObserver));
        RegisterObserver(Guard.AgainstNull(idempotenceObserver));
        RegisterObserver(Guard.AgainstNull(handleMessageObserver));
        RegisterObserver(Guard.AgainstNull(acknowledgeMessageObserver));
        RegisterObserver(Guard.AgainstNull(sendDeferredObserver));

        RegisterObserver(Guard.AgainstNull(receiveExceptionObserver)); // must be last

        Guard.AgainstNull(Guard.AgainstNull(serviceBusOptions).Value);
        Guard.AgainstNull(serviceBusConfiguration);

        State.SetWorkQueue(Guard.AgainstNull(serviceBusConfiguration.Inbox!.WorkQueue));
        State.SetDeferredQueue(serviceBusConfiguration.Inbox.DeferredQueue);
        State.SetErrorQueue(serviceBusConfiguration.Inbox.ErrorQueue);

        State.SetDurationToIgnoreOnFailure(serviceBusOptions.Value.Inbox!.DurationToIgnoreOnFailure);
        State.SetMaximumFailureCount(serviceBusOptions.Value.Inbox.MaximumFailureCount);
    }
}