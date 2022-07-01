using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public interface ISubscriptionService
    {
        IEnumerable<Uri> GetSubscriberUris(object message);
    }
}