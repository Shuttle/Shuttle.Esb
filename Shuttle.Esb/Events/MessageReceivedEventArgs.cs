using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class MessageReceivedEventArgs : EventArgs
{
    public MessageReceivedEventArgs(ReceivedMessage receivedMessage)
    {
        ReceivedMessage = Guard.AgainstNull(receivedMessage);
    }

    public ReceivedMessage ReceivedMessage { get; }
}