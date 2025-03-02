using System;

namespace Shuttle.Esb;

public class DeferredMessageProcessingAdjustedEventArgs : EventArgs
{
    public DeferredMessageProcessingAdjustedEventArgs(DateTimeOffset nextProcessingDateTime)
    {
        NextProcessingDateTime = nextProcessingDateTime;
    }

    public DateTimeOffset NextProcessingDateTime { get; }
}