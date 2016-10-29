using System.Security.Principal;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DefaultIdentityProvider : IIdentityProvider, IDependency<IServiceBus>
    {
        private static IIdentity _identity;
        private bool _cache;

        public IIdentity Get()
        {
            return _cache ? _identity : WindowsIdentity.GetCurrent();
        }

        public void Assign(IServiceBus dependency)
        {
            Guard.AgainstNull(dependency, "dependency");

            _cache = dependency.Configuration.CacheIdentity;

            if (_cache)
            {
                _identity = WindowsIdentity.GetCurrent();
            }
        }
    }
}