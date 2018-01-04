using Shuttle.Core.Contract;
using Shuttle.Core.PipelineTransaction;

namespace Shuttle.Esb
{
    public class ControlInboxMessagePipeline : ReceiveMessagePipeline
    {
        public ControlInboxMessagePipeline(IServiceBusConfiguration configuration,
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

            State.SetWorkQueue(configuration.ControlInbox.WorkQueue);
            State.SetErrorQueue(configuration.ControlInbox.ErrorQueue);
            State.SetDurationToIgnoreOnFailure(configuration.ControlInbox.DurationToIgnoreOnFailure);
            State.SetMaximumFailureCount(configuration.ControlInbox.MaximumFailureCount);
        }
    }
}