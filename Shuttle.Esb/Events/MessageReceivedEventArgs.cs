using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public ReceivedMessage ReceivedMessage { get; }

        public MessageReceivedEventArgs(ReceivedMessage receivedMessage)
        {
            ReceivedMessage = Guard.AgainstNull(receivedMessage, nameof(receivedMessage));
        }
    }
}