using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class TransportMessageDeferredEventArgs : EventArgs
    {
        public TransportMessageDeferredEventArgs(TransportMessage transportMessage)
        {
            Guard.AgainstNull(transportMessage, nameof(transportMessage));

            TransportMessage = transportMessage;
        }

        public TransportMessage TransportMessage { get; }
    }
}