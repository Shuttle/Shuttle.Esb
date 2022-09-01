using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public interface IQueueFactoryService
    {
        IQueueFactory Get(string scheme);
        IQueueFactory Get(Uri uri);
        IEnumerable<IQueueFactory> Factories { get; }
        void Register(IQueueFactory queueFactory);
        bool Contains(string scheme);
    }
}