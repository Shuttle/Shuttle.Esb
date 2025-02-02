using System.Text.RegularExpressions;
using Shuttle.Core.Contract;
using Shuttle.Core.Specification;

namespace Shuttle.Esb;

public class RegexMessageRouteSpecification : ISpecification<string>
{
    private readonly Regex _regex;

    public RegexMessageRouteSpecification(string pattern)
    {
        _regex = new(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    public bool IsSatisfiedBy(string messageType)
    {
        return _regex.IsMatch(Guard.AgainstNullOrEmptyString(messageType));
    }
}