using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public abstract class ReceiveMessagePipeline : Pipeline
    {
        protected ReceiveMessagePipeline(IEnumerable<IPipelineObserver> observers)
        {
            Guard.AgainstNull(observers, nameof(observers));

            var list = observers.ToList();

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

            RegisterObserver(list.Get<IGetWorkMessageObserver>());
            RegisterObserver(list.Get<IDeserializeTransportMessageObserver>());
            RegisterObserver(list.Get<IDeferTransportMessageObserver>());
            RegisterObserver(list.Get<IDeserializeMessageObserver>());
            RegisterObserver(list.Get<IDecryptMessageObserver>());
            RegisterObserver(list.Get<IDecompressMessageObserver>());
            RegisterObserver(list.Get<IAssessMessageHandlingObserver>());
            RegisterObserver(list.Get<IIdempotenceObserver>());
            RegisterObserver(list.Get<IHandleMessageObserver>());
            RegisterObserver(list.Get<ITransactionScopeObserver>());
            RegisterObserver(list.Get<IAcknowledgeMessageObserver>());
            RegisterObserver(list.Get<ISendDeferredObserver>());

            RegisterObserver(list.Get<IReceiveExceptionObserver>()); // must be last
        }
    }
}