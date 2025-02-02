using System;
using System.Collections.Generic;
using Shuttle.Core.Specification;

namespace Shuttle.Esb;

public interface IMessageRoute
{
    IEnumerable<ISpecification<string>> Specifications { get; }
    Uri Uri { get; }
    IMessageRoute AddSpecification(ISpecification<string> specification);
    bool IsSatisfiedBy(string messageType);
}