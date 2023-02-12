using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public class NullSubscriptionService : ISubscriptionService
    {
        public Task<IEnumerable<string>> GetSubscribedUris(object message)
        {
            throw new NotImplementedException("NullSubscriptionManager");
        }
    }
}