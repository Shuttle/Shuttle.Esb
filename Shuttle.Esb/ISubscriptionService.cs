using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public interface ISubscriptionService
    {
        Task<IEnumerable<string>> GetSubscribedUris(object message);
    }
}