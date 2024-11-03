using System;
using System.Collections.Generic;

namespace Shuttle.Esb;

public class MappedDelegateProvider : IMappedDelegateProvider
{
    public MappedDelegateProvider(IDictionary<Type, Delegate> delegates)
    {
        Delegates = delegates;
    }

    public IDictionary<Type, Delegate> Delegates { get; }
}