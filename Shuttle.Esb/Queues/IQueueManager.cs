using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    // IQueueRepository?
    // IQueueFactoryRepository?
    public interface IQueueManager
    {
        IQueueFactory GetQueueFactory(string scheme);
        IQueueFactory GetQueueFactory(Uri uri);
        IQueue GetQueue(string uri);
        IQueue CreateQueue(string uri);
        IQueue CreateQueue(Uri uri);
        IEnumerable<IQueueFactory> QueueFactories();
        void RegisterQueueFactory(IQueueFactory queueFactory);
        bool ContainsQueueFactory(string scheme);
        void CreatePhysicalQueues(IServiceBusConfiguration configuration);
    }
}