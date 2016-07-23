using System.Security.Principal;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DefaultIdentityProvider : IIdentityProvider, IRequireInitialization
    {
        private static IIdentity _identity;
        private bool _cache;

        public IIdentity Get()
        {
            return _cache ? _identity : WindowsIdentity.GetCurrent();
        }

        public void Initialize(IServiceBus bus)
        {
            Guard.AgainstNull(bus, "bus");

            _cache = bus.Configuration.CacheIdentity;

            if (_cache)
            {
                _identity = WindowsIdentity.GetCurrent();
            }
        }
    }
}