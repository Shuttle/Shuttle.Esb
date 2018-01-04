using System;
using System.Collections.Generic;
using Shuttle.Core.Specification;

namespace Shuttle.Esb
{
    public interface IMessageRoute
    {
        Uri Uri { get; }

        IEnumerable<ISpecification<string>> Specifications { get; }
        IMessageRoute AddSpecification(ISpecification<string> specification);
        bool IsSatisfiedBy(string messageType);
    }
}