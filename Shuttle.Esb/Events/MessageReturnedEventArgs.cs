using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class MessageReturnedEventArgs : EventArgs
    {
        public TransportMessage TransportMessage { get; }
        public ReceivedMessage ReceivedMessage { get; }

        public MessageReturnedEventArgs(TransportMessage transportMessage, ReceivedMessage receivedMessage)
        {
            TransportMessage = Guard.AgainstNull(transportMessage, nameof(transportMessage));
            ReceivedMessage = Guard.AgainstNull(receivedMessage, nameof(receivedMessage));
        }
    }
}