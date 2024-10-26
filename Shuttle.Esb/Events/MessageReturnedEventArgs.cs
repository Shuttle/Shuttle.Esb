using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class MessageReturnedEventArgs : EventArgs
{
    public MessageReturnedEventArgs(TransportMessage transportMessage, ReceivedMessage receivedMessage)
    {
        TransportMessage = Guard.AgainstNull(transportMessage);
        ReceivedMessage = Guard.AgainstNull(receivedMessage);
    }

    public ReceivedMessage ReceivedMessage { get; }
    public TransportMessage TransportMessage { get; }
}