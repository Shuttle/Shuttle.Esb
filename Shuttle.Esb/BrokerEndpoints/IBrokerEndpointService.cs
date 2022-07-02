using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public interface IBrokerEndpointService
    {
        IBrokerEndpointFactory GetBrokerEndpointFactory(string scheme);
        IBrokerEndpointFactory GetBrokerEndpointFactory(Uri uri);
        IBrokerEndpoint GetBrokerEndpoint(string uri);
        IBrokerEndpoint CreateBrokerEndpoint(string uri);
        IBrokerEndpoint CreateBrokerEndpoint(Uri uri);
        IEnumerable<IBrokerEndpointFactory> BrokerEndpointFactories();
        void RegisterBrokerEndpointFactory(IBrokerEndpointFactory brokerEndpointFactory);
        bool ContainsBrokerEndpointFactory(string scheme);
        void CreatePhysicalBrokerEndpoint(IServiceBusConfiguration configuration);
    }
}