using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public interface IBrokerEndpointFactoryService
    {
        IEnumerable<IBrokerEndpointFactory> Factories { get; }
        IBrokerEndpointFactory Get(string scheme);
        IBrokerEndpointFactory Get(Uri uri);
        void Register(IBrokerEndpointFactory brokerEndpointFactory);
        bool Contains(string scheme);
    }
}