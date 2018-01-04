using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Shuttle.Core.Contract;
using Shuttle.Core.Specification;

namespace Shuttle.Esb
{
    public class MessageRoute : IMessageRoute
    {
        private readonly List<ISpecification<string>> _specifications = new List<ISpecification<string>>();

        public MessageRoute(Uri uri)
        {
            Guard.AgainstNull(uri, nameof(uri));

            Uri = uri;
        }

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

        public Uri Uri { get; }

        public IEnumerable<ISpecification<string>> Specifications =>
            new ReadOnlyCollection<ISpecification<string>>(_specifications);
    }
}