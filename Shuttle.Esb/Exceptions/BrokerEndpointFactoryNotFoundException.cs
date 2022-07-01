using System;

namespace Shuttle.Esb
{
    public class BrokerEndpointFactoryNotFoundException : Exception
    {
        public BrokerEndpointFactoryNotFoundException(string scheme)
            : base(string.Format(Resources.BrokerEndpointFactoryNotFoundException, scheme))
        {
        }
    }
}