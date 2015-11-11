using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
    public interface IMessageRoute
    {
        IQueue Queue { get; }
        IMessageRoute AddSpecification(ISpecification<string> specification);
        bool IsSatisfiedBy(string messageType);

        IEnumerable<ISpecification<string>> Specifications { get; }
    }
}