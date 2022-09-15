using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public class NullSubscriptionService : ISubscriptionService
    {
        public IEnumerable<string> GetSubscribedUris(object message)
        {
            throw new NotImplementedException("NullSubscriptionManager");
        }
    }
}