using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Tests;

public class NullQueueFactory : IQueueFactory
{
    public string Scheme { get; } = "null-queue";

    public IQueue Create(Uri uri)
    {
        return new NullQueue(Guard.AgainstNull(uri));
    }
}