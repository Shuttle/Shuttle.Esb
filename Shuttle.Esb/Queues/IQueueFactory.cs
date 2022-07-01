using System;

namespace Shuttle.Esb
{
    // IBrokerFactory
    public interface IQueueFactory
    {
        string Scheme { get; }
        IQueue Create(Uri uri);
    }
}