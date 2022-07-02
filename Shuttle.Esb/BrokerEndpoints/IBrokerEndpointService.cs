using System;

namespace Shuttle.Esb
{
    public interface IBrokerEndpointService
    {
        IBrokerEndpoint Get(string uri);
        IBrokerEndpoint Create(string uri);
        IBrokerEndpoint Create(Uri uri);
        void CreatePhysical(IServiceBusConfiguration configuration);
        bool Contains(string uri);
    }
}