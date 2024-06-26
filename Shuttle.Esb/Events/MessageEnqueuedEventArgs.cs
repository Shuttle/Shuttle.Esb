using System;
using System.IO;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class MessageEnqueuedEventArgs : EventArgs
    {
        public TransportMessage TransportMessage { get; }
        public Stream Stream { get; }

        public MessageEnqueuedEventArgs(TransportMessage message, Stream stream)
        {
            TransportMessage = Guard.AgainstNull(message, nameof(message));
            Stream = Guard.AgainstNull(stream, nameof(stream));
        }
    }
}