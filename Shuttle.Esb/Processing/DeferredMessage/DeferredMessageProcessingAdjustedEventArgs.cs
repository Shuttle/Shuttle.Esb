using System;

namespace Shuttle.Esb
{
    public class DeferredMessageProcessingAdjustedEventArgs : EventArgs
    {
        public DateTime NextProcessingDateTime { get; }

        public DeferredMessageProcessingAdjustedEventArgs(DateTime nextProcessingDateTime)
        {
            NextProcessingDateTime = nextProcessingDateTime;
        }
    }
}