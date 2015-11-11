using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
    public class StartsWithMessageRouteSpecification : ISpecification<string>
    {
        private readonly string _startWith;

        public StartsWithMessageRouteSpecification(string startWith)
        {
            this._startWith = startWith.ToLower();
        }

        public bool IsSatisfiedBy(string messageType)
        {
            Guard.AgainstNull(messageType, "message");

            return messageType.ToLower().StartsWith(_startWith);
        }
    }
}