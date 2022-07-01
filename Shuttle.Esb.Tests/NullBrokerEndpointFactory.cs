using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Tests
{
    public class NullBrokerEndpointFactory : IBrokerEndpointFactory
    {
        public NullBrokerEndpointFactory()
        {
            Scheme = "null-queue";
        }

        public string Scheme { get; }

        public IBrokerEndpoint Create(Uri uri)
        {
            Guard.AgainstNull(uri, nameof(uri));

            return new NullBrokerEndpoint(uri);
        }
    }
}