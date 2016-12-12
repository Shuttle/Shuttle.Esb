using System.Collections.Generic;
using System.Collections.ObjectModel;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class MessageRouteConfiguration
    {
        private readonly List<MessageRouteSpecificationConfiguration> _specifications =
            new List<MessageRouteSpecificationConfiguration>();

        public MessageRouteConfiguration(string uri)
        {
            Uri = uri;
        }

        public string Uri { get; private set; }

        public IEnumerable<MessageRouteSpecificationConfiguration> Specifications
        {
            get { return new ReadOnlyCollection<MessageRouteSpecificationConfiguration>(_specifications); }
        }

        public void AddSpecification(string name, string value)
        {
            Guard.AgainstNullOrEmptyString(name, "name");
            Guard.AgainstNullOrEmptyString(value, "value");

            _specifications.Add(new MessageRouteSpecificationConfiguration(name, value));
        }
    }
}