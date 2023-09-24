using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public interface ISubscriptionService
    {
        IEnumerable<string> GetSubscribedUris(object message);
        Task<IEnumerable<string>> GetSubscribedUrisAsync(object message);
    }
}