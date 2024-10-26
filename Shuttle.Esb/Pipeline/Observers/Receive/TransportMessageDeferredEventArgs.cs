using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class TransportMessageDeferredEventArgs : EventArgs
{
    public TransportMessageDeferredEventArgs(TransportMessage transportMessage)
    {
        TransportMessage = Guard.AgainstNull(transportMessage);
    }

    public TransportMessage TransportMessage { get; }
}