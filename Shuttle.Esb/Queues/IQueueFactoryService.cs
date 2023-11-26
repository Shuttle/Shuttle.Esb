using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public interface IQueueFactoryService : IDisposable, IAsyncDisposable
    {
        IQueueFactory Get(string scheme);
        IEnumerable<IQueueFactory> Factories { get; }
        void Register(IQueueFactory queueFactory);
        bool Contains(string scheme);
    }
}