using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public class NullSubscriptionService : ISubscriptionService
    {
        public IEnumerable<string> GetSubscribedUris(object message)
        {
            throw new NotImplementedException("NullSubscriptionManager");
        }

        public Task<IEnumerable<string>> GetSubscribedUrisAsync(object message)
        {
            throw new NotImplementedException("NullSubscriptionManager");
        }
    }
}