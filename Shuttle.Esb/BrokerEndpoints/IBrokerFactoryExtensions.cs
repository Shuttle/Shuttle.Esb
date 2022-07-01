using System;

namespace Shuttle.Esb
{
    public static class IBrokerFactoryExtensions
    {
        public static bool CanCreate(this IBrokerEndpointFactory factory, Uri uri)
        {
            return uri.Scheme.Equals(factory.Scheme, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}