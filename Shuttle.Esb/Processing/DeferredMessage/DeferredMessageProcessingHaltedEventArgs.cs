using System;

namespace Shuttle.Esb;

public class DeferredMessageProcessingHaltedEventArgs : EventArgs
{
    public DeferredMessageProcessingHaltedEventArgs(DateTimeOffset restartDateTime)
    {
        RestartDateTime = restartDateTime;
    }

    public DateTimeOffset RestartDateTime { get; }
}