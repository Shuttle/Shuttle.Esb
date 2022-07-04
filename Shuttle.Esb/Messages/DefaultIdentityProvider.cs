using System;
using System.Security.Principal;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class DefaultIdentityProvider : IIdentityProvider
    {
        private static IIdentity _identity;
        private readonly bool _cache;

        public DefaultIdentityProvider(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _cache = configuration.ShouldCacheIdentity;

            if (_cache)
            {
                _identity = new GenericIdentity(Environment.UserDomainName + "\\" + Environment.UserName, "Anonymous");
            }
        }

        public IIdentity Get()
        {
            return _cache ? _identity : new GenericIdentity(Environment.UserDomainName + "\\" + Environment.UserName, "Anonymous");
        }
    }
}