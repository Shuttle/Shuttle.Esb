using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class ControlInboxMessagePipeline : ReceiveMessagePipeline
    {
        public ControlInboxMessagePipeline(IServiceBus bus, GetWorkMessageObserver getWorkMessageObserver,
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
            Guard.AgainstNull(bus, "bus");

            State.SetServiceBus(bus);

            State.SetWorkQueue(bus.Configuration.ControlInbox.WorkQueue);
            State.SetErrorQueue(bus.Configuration.ControlInbox.ErrorQueue);
            State.SetDurationToIgnoreOnFailure(bus.Configuration.ControlInbox.DurationToIgnoreOnFailure);
            State.SetMaximumFailureCount(bus.Configuration.ControlInbox.MaximumFailureCount);
        }
    }
}