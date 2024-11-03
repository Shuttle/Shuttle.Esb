using System.Collections.Generic;
using System;

namespace Shuttle.Esb;

public interface IMappedDelegateProvider
{
    IDictionary<Type, Delegate> Delegates { get; }
}