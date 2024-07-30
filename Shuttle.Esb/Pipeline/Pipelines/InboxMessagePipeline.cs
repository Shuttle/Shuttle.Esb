using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransactionScope;

namespace Shuttle.Esb
{
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

            RegisterObserver(Guard.AgainstNull(getWorkMessageObserver, nameof(getWorkMessageObserver)));
            RegisterObserver(Guard.AgainstNull(deserializeTransportMessageObserver, nameof(deserializeTransportMessageObserver)));
            RegisterObserver(Guard.AgainstNull(deferTransportMessageObserver, nameof(deferTransportMessageObserver)));
            RegisterObserver(Guard.AgainstNull(deserializeMessageObserver, nameof(deserializeMessageObserver)));
            RegisterObserver(Guard.AgainstNull(decryptMessageObserver, nameof(decryptMessageObserver)));
            RegisterObserver(Guard.AgainstNull(decompressMessageObserver, nameof(decompressMessageObserver)));
            RegisterObserver(Guard.AgainstNull(messageHandlingSpecificationObserver, nameof(messageHandlingSpecificationObserver)));
            RegisterObserver(Guard.AgainstNull(idempotenceObserver, nameof(idempotenceObserver)));
            RegisterObserver(Guard.AgainstNull(handleMessageObserver, nameof(handleMessageObserver)));
            RegisterObserver(Guard.AgainstNull(acknowledgeMessageObserver, nameof(acknowledgeMessageObserver)));
            RegisterObserver(Guard.AgainstNull(sendDeferredObserver, nameof(sendDeferredObserver)));

            RegisterObserver(Guard.AgainstNull(receiveExceptionObserver, nameof(receiveExceptionObserver))); // must be last

            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));

            State.SetWorkQueue(serviceBusConfiguration.Inbox.WorkQueue);
            State.SetDeferredQueue(serviceBusConfiguration.Inbox.DeferredQueue);
            State.SetErrorQueue(serviceBusConfiguration.Inbox.ErrorQueue);

            State.SetDurationToIgnoreOnFailure(serviceBusOptions.Value.Inbox.DurationToIgnoreOnFailure);
            State.SetMaximumFailureCount(serviceBusOptions.Value.Inbox.MaximumFailureCount);
        }
    }
}