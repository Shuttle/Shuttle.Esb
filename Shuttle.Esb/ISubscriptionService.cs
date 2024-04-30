using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public interface ISubscriptionService
    {
        IEnumerable<string> GetSubscribedUris(string messageType);
        Task<IEnumerable<string>> GetSubscribedUrisAsync(string messageType);
    }
}