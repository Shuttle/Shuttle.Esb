using System.Collections.Generic;

namespace Shuttle.Esb
{
    public interface ISubscriptionService
    {
        IEnumerable<string> GetSubscribedUris(object message);
    }
}