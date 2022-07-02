using System.Collections.Generic;

namespace Shuttle.Esb
{
    public interface ISubscriptionManager
    {
        IEnumerable<string> GetSubscribedUris(object message);
    }
}