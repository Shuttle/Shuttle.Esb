using System;
using System.IO;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class MessageEnqueuedEventArgs : EventArgs
{
    public MessageEnqueuedEventArgs(TransportMessage message, Stream stream)
    {
        TransportMessage = Guard.AgainstNull(message);
        Stream = Guard.AgainstNull(stream);
    }

    public Stream Stream { get; }
    public TransportMessage TransportMessage { get; }
}