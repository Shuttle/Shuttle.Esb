using System;

namespace Shuttle.Esb
{
    public class DeferredMessageProcessingHaltedEventArgs : EventArgs
    {
        public DateTime RestartDateTime { get; }

        public DeferredMessageProcessingHaltedEventArgs(DateTime restartDateTime)
        {
            RestartDateTime = restartDateTime;
        }
    }
}