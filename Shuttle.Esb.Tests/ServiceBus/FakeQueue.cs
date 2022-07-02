using System;
using System.IO;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;

namespace Shuttle.Esb.Tests
{
    public class FakeQueue : IQueue
    {
        private readonly ISerializer _serializer = new DefaultSerializer();

        public FakeQueue(int messagesToReturn)
        {
            MessagesToReturn = messagesToReturn;
        }

        public int MessagesToReturn { get; }

        public int MessageCount { get; private set; }

        public Uri Uri { get; }

        public bool IsEmpty()
        {
            return false;
        }

        public void Enqueue(TransportMessage message, Stream stream)
        {
        }

        public ReceivedMessage GetMessage()
        {
            if (MessageCount == MessagesToReturn)
            {
                return null;
            }

            var expired = MessageCount % 2 != 0;

            var command = new SimpleCommand(expired ? "Expired" : "HasNotExpired");

            var transportMessage = new TransportMessage
            {
                MessageType = command.GetType().Name,
                ExpiryDate = expired ? DateTime.Now.AddMilliseconds(-1) : DateTime.MaxValue,
                PrincipalIdentityName = "Identity",
                AssemblyQualifiedName = command.GetType().AssemblyQualifiedName,
                Message = _serializer.Serialize(command).ToBytes()
            };

            MessageCount += 1;

            return new ReceivedMessage(_serializer.Serialize(transportMessage), null);
        }

        public void Acknowledge(object acknowledgementToken)
        {
        }

        public void Release(object acknowledgementToken)
        {
        }
    }
}