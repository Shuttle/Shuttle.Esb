using System;
using System.IO;

namespace Shuttle.Esb
{
    public interface IBrokerEndpoint
    {
        Uri Uri { get; }

        bool IsEmpty();

        void Send(TransportMessage message, Stream stream);
        ReceivedMessage Receive();
    }
}