using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public class NullSubscriptionService : ISubscriptionService
    {
        public void Subscribe(IEnumerable<string> messageTypeFullNames)
        {
            throw new NotImplementedException("NullSubscriptionManager");
        }

        public void Subscribe(string messageTypeFullName)
        {
            throw new NotImplementedException("NullSubscriptionManager");
        }

        public void Subscribe(IEnumerable<Type> messageTypes)
        {
            throw new NotImplementedException("NullSubscriptionManager");
        }

        public void Subscribe(Type messageType)
        {
            throw new NotImplementedException("NullSubscriptionManager");
        }

        public void Subscribe<T>()
        {
            throw new NotImplementedException("NullSubscriptionManager");
        }

        public IEnumerable<string> GetSubscribedUris(object message)
        {
            throw new NotImplementedException("NullSubscriptionManager");
        }
    }
}