using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class InboxMessagePipeline : ReceiveMessagePipeline
	{
	    public InboxMessagePipeline(IServiceBus bus, GetWorkMessageObserver getWorkMessageObserver,
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

            State.SetWorkQueue(bus.Configuration.Inbox.WorkQueue);
            State.SetDeferredQueue(bus.Configuration.Inbox.DeferredQueue);
            State.SetErrorQueue(bus.Configuration.Inbox.ErrorQueue);

            State.SetDurationToIgnoreOnFailure(bus.Configuration.Inbox.DurationToIgnoreOnFailure);
            State.SetMaximumFailureCount(bus.Configuration.Inbox.MaximumFailureCount);
        }
    }
}