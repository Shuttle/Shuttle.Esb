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
            Guard.AgainstNull(getWorkMessageObserver, nameof(getWorkMessageObserver));
            Guard.AgainstNull(deserializeTransportMessageObserver, nameof(deserializeTransportMessageObserver));
            Guard.AgainstNull(deferTransportMessageObserver, nameof(deferTransportMessageObserver));
            Guard.AgainstNull(deserializeMessageObserver, nameof(deserializeMessageObserver));
            Guard.AgainstNull(decryptMessageObserver, nameof(decryptMessageObserver));
            Guard.AgainstNull(decompressMessageObserver, nameof(decompressMessageObserver));
            Guard.AgainstNull(assessMessageHandlingObserver, nameof(assessMessageHandlingObserver));
            Guard.AgainstNull(idempotenceObserver, nameof(idempotenceObserver));
            Guard.AgainstNull(handleMessageObserver, nameof(handleMessageObserver));
            Guard.AgainstNull(acknowledgeMessageObserver, nameof(acknowledgeMessageObserver));
            Guard.AgainstNull(sendDeferredObserver, nameof(sendDeferredObserver));
            Guard.AgainstNull(receiveExceptionObserver, nameof(receiveExceptionObserver));

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

            RegisterObserver(getWorkMessageObserver);
            RegisterObserver(deserializeTransportMessageObserver);
            RegisterObserver(deferTransportMessageObserver);
            RegisterObserver(deserializeMessageObserver);
            RegisterObserver(decryptMessageObserver);
            RegisterObserver(decompressMessageObserver);
            RegisterObserver(assessMessageHandlingObserver);
            RegisterObserver(idempotenceObserver);
            RegisterObserver(handleMessageObserver);
            RegisterObserver(acknowledgeMessageObserver);
            RegisterObserver(sendDeferredObserver);
            RegisterObserver(transactionScopeObserver);

            RegisterObserver(receiveExceptionObserver); // must be last
        }
    }
}