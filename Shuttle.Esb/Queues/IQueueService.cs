using System;

namespace Shuttle.Esb
{
    public interface IQueueService
    {
        IQueue Get(string uri);
        IQueue Create(string uri);
        IQueue Create(Uri uri);
        void CreatePhysicalQueues(IServiceBusConfiguration serviceBusConfiguration);
        bool Contains(string uri);
    }
}