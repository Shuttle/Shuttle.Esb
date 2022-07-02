using Shuttle.Core.Contract;
using Shuttle.Core.PipelineTransaction;

namespace Shuttle.Esb
{
    public class InboxMessagePipeline : ReceiveMessagePipeline
    {
        public InboxMessagePipeline(IServiceBusConfiguration configuration,
            IGetWorkMessageObserver getWorkMessageObserver,
            IDeserializeTransportMessageObserver deserializeTransportMessageObserver,
            IDeferTransportMessageObserver deferTransportMessageObserver,
            IDeserializeMessageObserver deserializeMessageObserver, IDecryptMessageObserver decryptMessageObserver,
            IDecompressMessageObserver decompressMessageObserver,
            IAssessMessageHandlingObserver assessMessageHandlingObserver, IIdempotenceObserver idempotenceObserver,
            IHandleMessageObserver handleMessageObserver, IAcknowledgeMessageObserver acknowledgeMessageObserver,
            ISendDeferredObserver sendDeferredObserver, IReceiveExceptionObserver receiveExceptionObserver,
            ITransactionScopeObserver transactionScopeObserver)
            : base(getWorkMessageObserver, deserializeTransportMessageObserver, deferTransportMessageObserver,
                deserializeMessageObserver, decryptMessageObserver, decompressMessageObserver,
                assessMessageHandlingObserver, idempotenceObserver, handleMessageObserver, acknowledgeMessageObserver,
                sendDeferredObserver, receiveExceptionObserver, transactionScopeObserver)
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