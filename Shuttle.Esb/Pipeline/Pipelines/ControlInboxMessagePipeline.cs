using Shuttle.Core.Contract;
using Shuttle.Core.PipelineTransaction;

namespace Shuttle.Esb
{
    public class ControlInboxMessagePipeline : ReceiveMessagePipeline
    {
        public ControlInboxMessagePipeline(IServiceBusConfiguration configuration,
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

            State.SetWorkQueue(configuration.ControlInbox.WorkQueue);
            State.SetErrorQueue(configuration.ControlInbox.ErrorQueue);
            State.SetDurationToIgnoreOnFailure(configuration.ControlInbox.DurationToIgnoreOnFailure);
            State.SetMaximumFailureCount(configuration.ControlInbox.MaximumFailureCount);
        }
    }
}