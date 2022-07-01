using System;

namespace Shuttle.Esb
{
    public interface IBrokerEndpointFactory
    {
        string Scheme { get; }
        IBrokerEndpoint Create(Uri uri);
    }
}