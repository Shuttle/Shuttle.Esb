using System.Collections.Generic;
using System.Collections.ObjectModel;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class MessageRoute : IMessageRoute
    {
        private readonly List<ISpecification<string>> _specifications = new List<ISpecification<string>>();

        public MessageRoute(IQueue queue)
        {
            Queue = queue;
        }

        public IQueue Queue { get; private set; }

        public IMessageRoute AddSpecification(ISpecification<string> specification)
        {
            _specifications.Add(specification);

            return this;
        }

        public bool IsSatisfiedBy(string messageType)
        {
            foreach (var specification in _specifications)
            {
                if (specification.IsSatisfiedBy(messageType))
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<ISpecification<string>> Specifications
        {
            get
            {
                return new ReadOnlyCollection<ISpecification<string>>(_specifications);
            }
        }
    }
}