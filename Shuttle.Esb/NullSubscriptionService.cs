using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public class NullSubscriptionService : ISubscriptionService
    {
        public IEnumerable<Uri> GetSubscriberUris(object message)
        {
            throw new NotImplementedException("NullSubscriptionService");
        }
    }
}