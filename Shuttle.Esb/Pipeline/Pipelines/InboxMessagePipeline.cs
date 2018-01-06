using System.Collections.Generic;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;

namespace Shuttle.Esb
{
    public class InboxMessagePipeline : ReceiveMessagePipeline
    {
        public InboxMessagePipeline(IServiceBusConfiguration configuration, IEnumerable<IPipelineObserver> observers, ITransactionScopeObserver transactionScopeObserver)
            : base(observers, transactionScopeObserver)
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