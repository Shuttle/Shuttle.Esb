using Shuttle.Core.Contract;
using Shuttle.Core.PipelineTransaction;

namespace Shuttle.Esb
{
    public class InboxMessagePipeline : ReceiveMessagePipeline
    {
        public InboxMessagePipeline(IServiceBusConfiguration configuration,
            GetWorkMessageObserver getWorkMessageObserver,
            DeserializeTransportMessageObserver deserializeTransportMessageObserver,
            DeferTransportMessageObserver deferTransportMessageObserver,
            DeserializeMessageObserver deserializeMessageObserver, DecryptMessageObserver decryptMessageObserver,
            DecompressMessageObserver decompressMessageObserver,
            AssessMessageHandlingObserver assessMessageHandlingObserver, IdempotenceObserver idempotenceObserver,
            HandleMessageObserver handleMessageObserver, TransactionScopeObserver transactionScopeObserver,
            AcknowledgeMessageObserver acknowledgeMessageObserver,
            SendDeferredObserver sendDeferredObserver, ReceiveExceptionObserver receiveExceptionObserver)
            : base(
                getWorkMessageObserver, deserializeTransportMessageObserver, deferTransportMessageObserver,
                deserializeMessageObserver, decryptMessageObserver, decompressMessageObserver,
                assessMessageHandlingObserver, idempotenceObserver, handleMessageObserver, transactionScopeObserver,
                acknowledgeMessageObserver, sendDeferredObserver, receiveExceptionObserver)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            State.SetWorkQueue(configuration.Inbox.WorkQueue);
            State.SetDeferredQueue(configuration.Inbox.DeferredQueue);
            State.SetErrorQueue(configuration.Inbox.ErrorQueue);

            State.SetDurationToIgnoreOnFailure(configuration.Inbox.DurationToIgnoreOnFailure);
            State.SetMaximumFailureCount(configuration.Inbox.MaximumFailureCount);
        }
    }
}