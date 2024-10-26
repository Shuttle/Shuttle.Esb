using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb;

public class QueueFactoryService : IQueueFactoryService
{
    private readonly List<IQueueFactory> _queueFactories = new();
    private bool _disposed;

    public QueueFactoryService(IEnumerable<IQueueFactory>? queueFactories = null)
    {
        _queueFactories.AddRange(queueFactories ?? Enumerable.Empty<IQueueFactory>());
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var queueFactory in _queueFactories)
        {
            queueFactory.TryDispose();
        }

        _queueFactories.Clear();

        _disposed = true;
    }

    public IQueueFactory Get(string scheme)
    {
        Guard.AgainstNullOrEmptyString(scheme);

        return Find(scheme) ?? throw new QueueFactoryNotFoundException(scheme);
    }

    public IEnumerable<IQueueFactory> Factories => _queueFactories.AsReadOnly();

    public IQueueFactory? Find(string scheme)
    {
        Guard.AgainstNullOrEmptyString(scheme);

        return _queueFactories.FirstOrDefault(factory => factory.Scheme.Equals(scheme, StringComparison.InvariantCultureIgnoreCase));
    }

    public void Register(IQueueFactory queueFactory)
    {
        var factory = Find(Guard.AgainstNull(queueFactory).Scheme);

        if (factory != null)
        {
            _queueFactories.Remove(factory);
        }

        _queueFactories.Add(queueFactory);
    }

    public bool Contains(string scheme)
    {
        return Find(scheme) != null;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();

        return new();
    }
}