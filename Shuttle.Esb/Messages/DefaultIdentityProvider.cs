using System;
using System.Security.Principal;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class DefaultIdentityProvider : IIdentityProvider
{
    private static IIdentity? _identity;
    private readonly bool _cache;

    public DefaultIdentityProvider(IOptions<ServiceBusOptions> serviceBusOptions)
    {
        _cache = Guard.AgainstNull(Guard.AgainstNull(serviceBusOptions).Value).CacheIdentity;

        if (_cache)
        {
            _identity = new GenericIdentity(Environment.UserDomainName + "\\" + Environment.UserName, "Anonymous");
        }
    }

    public IIdentity Get()
    {
        return _cache ? _identity! : new GenericIdentity(Environment.UserDomainName + "\\" + Environment.UserName, "Anonymous");
    }
}