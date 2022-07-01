using System;
using System.IO;

namespace Shuttle.Esb
{
    public interface IBrokerEndpoint
    {
        Uri Uri { get; }

        bool IsEmpty();

        void Enqueue(TransportMessage message, Stream stream);
        ReceivedMessage GetMessage();
        void Acknowledge(object acknowledgementToken);
        void Release(object acknowledgementToken);
    }
}