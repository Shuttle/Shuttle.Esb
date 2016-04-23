using System.Text.RegularExpressions;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class RegexMessageRouteSpecification : ISpecification<string>
	{
		private readonly Regex _regex;

		public RegexMessageRouteSpecification(string pattern)
		{
			_regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}

		public bool IsSatisfiedBy(string messageType)
		{
			Guard.AgainstNull(messageType, "message");

			return _regex.IsMatch(messageType);
		}
	}
}