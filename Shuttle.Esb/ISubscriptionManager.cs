using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public interface ISubscriptionManager
    {
        void Subscribe(IEnumerable<string> messageTypeFullNames);
        void Subscribe(string messageTypeFullName);
        void Subscribe(IEnumerable<Type> messageTypes);
        void Subscribe(Type messageType);
        void Subscribe<T>();

        IEnumerable<string> GetSubscribedUris(object message);
    }
}