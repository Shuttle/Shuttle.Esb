using System;

namespace Shuttle.Esb;

public class DeferredMessageProcessingHaltedEventArgs : EventArgs
{
    public DeferredMessageProcessingHaltedEventArgs(DateTime restartDateTime)
    {
        RestartDateTime = restartDateTime;
    }

    public DateTime RestartDateTime { get; }
}