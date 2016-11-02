using System.Security.Principal;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DefaultIdentityProvider : IIdentityProvider
    {
        private static IIdentity _identity;
        private readonly bool _cache;

        public DefaultIdentityProvider(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, "configuration");

            _cache = configuration.CacheIdentity;

            if (_cache)
            {
                _identity = WindowsIdentity.GetCurrent();
            }
        }

        public IIdentity Get()
        {
            return _cache ? _identity : WindowsIdentity.GetCurrent();
        }
    }
}