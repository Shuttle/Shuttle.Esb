using System;
using System.IO;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ResolvedBrokerEndpoint : IBrokerEndpoint
    {
        private readonly IBrokerEndpoint _brokerEndpoint;

        public ResolvedBrokerEndpoint(IBrokerEndpoint brokerEndpoint, Uri uri)
        {
            Guard.AgainstNull(brokerEndpoint, nameof(brokerEndpoint));
            Guard.AgainstNull(uri, nameof(uri));

            _brokerEndpoint = brokerEndpoint;
            Uri = uri;
        }

        public Uri Uri { get; }

        public bool IsEmpty()
        {
            return _brokerEndpoint.IsEmpty();
        }

        public void Enqueue(TransportMessage transportMessage, Stream stream)
        {
            _brokerEndpoint.Enqueue(transportMessage, stream);
        }

        public ReceivedMessage GetMessage()
        {
            return _brokerEndpoint.GetMessage();
        }

        public void Acknowledge(object acknowledgementToken)
        {
            _brokerEndpoint.Acknowledge(acknowledgementToken);
        }

        public void Release(object acknowledgementToken)
        {
            _brokerEndpoint.Release(acknowledgementToken);
        }
    }
}