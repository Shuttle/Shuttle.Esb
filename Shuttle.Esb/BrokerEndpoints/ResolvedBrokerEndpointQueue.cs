using System;
using System.IO;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ResolvedBrokerEndpointQueue : ResolvedBrokerEndpoint, IQueue
    {
        private readonly IQueue _brokerEndpoint;

        public ResolvedBrokerEndpointQueue(IQueue brokerEndpoint, Uri uri) : base(brokerEndpoint, uri)
        {
            _brokerEndpoint = brokerEndpoint;
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