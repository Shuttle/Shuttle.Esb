using Shuttle.Core.Contract;
using Shuttle.Core.Specification;

namespace Shuttle.Esb;

public class StartsWithMessageRouteSpecification : ISpecification<string>
{
    private readonly string _startWith;

    public StartsWithMessageRouteSpecification(string startWith)
    {
        _startWith = Guard.AgainstNullOrEmptyString(startWith).ToLower();
    }

    public bool IsSatisfiedBy(string messageType)
    {
        return Guard.AgainstNull(messageType).ToLower().StartsWith(_startWith);
    }
}