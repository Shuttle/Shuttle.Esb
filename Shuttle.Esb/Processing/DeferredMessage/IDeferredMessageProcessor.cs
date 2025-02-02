using System;
using System.Threading.Tasks;
using Shuttle.Core.Threading;

namespace Shuttle.Esb;

public interface IDeferredMessageProcessor : IProcessor
{
    event EventHandler<DeferredMessageProcessingAdjustedEventArgs> DeferredMessageProcessingAdjusted;
    event EventHandler<DeferredMessageProcessingHaltedEventArgs> DeferredMessageProcessingHalted;

    Task MessageDeferredAsync(DateTime ignoreTillDate);
}