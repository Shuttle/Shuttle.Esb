using System;
using System.Collections.Generic;

namespace Shuttle.Esb;

public interface IQueueFactoryService : IDisposable, IAsyncDisposable
{
    IEnumerable<IQueueFactory> Factories { get; }
    bool Contains(string scheme);
    IQueueFactory? Find(string scheme);
    IQueueFactory Get(string scheme);
    void Register(IQueueFactory queueFactory);
}