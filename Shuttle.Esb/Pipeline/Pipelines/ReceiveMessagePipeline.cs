using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;

namespace Shuttle.Esb
{
    public abstract class ReceiveMessagePipeline : Pipeline
    {
        protected ReceiveMessagePipeline(GetWorkMessageObserver getWorkMessageObserver,
            DeserializeTransportMessageObserver deserializeTransportMessageObserver,
            DeferTransportMessageObserver deferTransportMessageObserver,
            DeserializeMessageObserver deserializeMessageObserver, DecryptMessageObserver decryptMessageObserver,
            DecompressMessageObserver decompressMessageObserver,
            AssessMessageHandlingObserver assessMessageHandlingObserver, IdempotenceObserver idempotenceObserver,
            HandleMessageObserver handleMessageObserver, TransactionScopeObserver transactionScopeObserver,
            AcknowledgeMessageObserver acknowledgeMessageObserver,
            SendDeferredObserver sendDeferredObserver, ReceiveExceptionObserver receiveExceptionObserver)
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

            RegisterObserver(getWorkMessageObserver);
            RegisterObserver(deserializeTransportMessageObserver);
            RegisterObserver(deferTransportMessageObserver);
            RegisterObserver(deserializeMessageObserver);
            RegisterObserver(decryptMessageObserver);
            RegisterObserver(decompressMessageObserver);
            RegisterObserver(assessMessageHandlingObserver);
            RegisterObserver(idempotenceObserver);
            RegisterObserver(handleMessageObserver);
            RegisterObserver(transactionScopeObserver);
            RegisterObserver(acknowledgeMessageObserver);
            RegisterObserver(sendDeferredObserver);

            RegisterObserver(receiveExceptionObserver); // must be last
        }
    }
}