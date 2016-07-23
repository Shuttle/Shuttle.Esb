using System.Security.Principal;

namespace Shuttle.Esb
{
    public interface IIdentityProvider
    {
        IIdentity Get();
    }
}