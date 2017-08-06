using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class StartsWithMessageRouteSpecification : ISpecification<string>
    {
        private readonly string _startWith;

        public StartsWithMessageRouteSpecification(string startWith)
        {
            _startWith = startWith.ToLower();
        }

        public bool IsSatisfiedBy(string messageType)
        {
            Guard.AgainstNull(messageType, nameof(messageType));

            return messageType.ToLower().StartsWith(_startWith);
        }
    }
}