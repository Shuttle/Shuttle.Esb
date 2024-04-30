using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.PipelineTransaction;

namespace Shuttle.Esb
{
    public class InboxMessagePipeline : ReceiveMessagePipeline
    {
        public InboxMessagePipeline(IOptions<ServiceBusOptions> serviceBusOptions, IServiceBusConfiguration serviceBusConfiguration,
            IGetWorkMessageObserver getWorkMessageObserver,
            IDeserializeTransportMessageObserver deserializeTransportMessageObserver,
            IDeferTransportMessageObserver deferTransportMessageObserver,
            IDeserializeMessageObserver deserializeMessageObserver, IDecryptMessageObserver decryptMessageObserver,
            IDecompressMessageObserver decompressMessageObserver,
            IMessageHandlingSpecificationObserver messageHandlingSpecificationObserver, IIdempotenceObserver idempotenceObserver,
            IHandleMessageObserver handleMessageObserver, IAcknowledgeMessageObserver acknowledgeMessageObserver,
            ISendDeferredObserver sendDeferredObserver, IReceiveExceptionObserver receiveExceptionObserver,
            ITransactionScopeObserver transactionScopeObserver)
            : base(getWorkMessageObserver, deserializeTransportMessageObserver, deferTransportMessageObserver,
                deserializeMessageObserver, decryptMessageObserver, decompressMessageObserver,
                messageHandlingSpecificationObserver, idempotenceObserver, handleMessageObserver, acknowledgeMessageObserver,
                sendDeferredObserver, receiveExceptionObserver, transactionScopeObserver)
        {
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