using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;

namespace Shuttle.Esb
{
    public abstract class ReceiveMessagePipeline : Pipeline
    {
        protected ReceiveMessagePipeline(IGetWorkMessageObserver getWorkMessageObserver,
            IDeserializeTransportMessageObserver deserializeTransportMessageObserver,
            IDeferTransportMessageObserver deferTransportMessageObserver,
            IDeserializeMessageObserver deserializeMessageObserver, IDecryptMessageObserver decryptMessageObserver,
            IDecompressMessageObserver decompressMessageObserver,
            IAssessMessageHandlingObserver assessMessageHandlingObserver, IIdempotenceObserver idempotenceObserver,
            IHandleMessageObserver handleMessageObserver, IAcknowledgeMessageObserver acknowledgeMessageObserver,
            ISendDeferredObserver sendDeferredObserver, IReceiveExceptionObserver receiveExceptionObserver,
            ITransactionScopeObserver transactionScopeObserver)
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
                .WithEvent<OnStartTransactionScope>()
                .WithEvent<OnAssessMessageHandling>()
                .WithEvent<OnAfterAssessMessageHandling>()
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
            RegisterObserver(Guard.AgainstNull(assessMessageHandlingObserver, nameof(assessMessageHandlingObserver)));
            RegisterObserver(Guard.AgainstNull(idempotenceObserver, nameof(idempotenceObserver)));
            RegisterObserver(Guard.AgainstNull(handleMessageObserver, nameof(handleMessageObserver)));
            RegisterObserver(Guard.AgainstNull(acknowledgeMessageObserver, nameof(acknowledgeMessageObserver)));
            RegisterObserver(Guard.AgainstNull(sendDeferredObserver, nameof(sendDeferredObserver)));
            RegisterObserver(Guard.AgainstNull(transactionScopeObserver, nameof(transactionScopeObserver)));

            RegisterObserver(Guard.AgainstNull(receiveExceptionObserver, nameof(receiveExceptionObserver))); // must be last
        }
    }
}