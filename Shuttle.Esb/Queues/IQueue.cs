using System;
using System.IO;

namespace Shuttle.Esb
{
    public interface IQueue
    {
        Uri Uri { get; }
        bool IsStream { get; }
        bool IsEmpty();
        void Enqueue(TransportMessage message, Stream stream);
        ReceivedMessage GetMessage();
        void Acknowledge(object acknowledgementToken);
        void Release(object acknowledgementToken);
    }
}