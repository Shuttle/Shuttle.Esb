using System;
using System.Linq;

namespace Shuttle.Esb
{
    public class MessageRouteSettings
    {
        public string Uri { get; set; }

        public SpecificationSettings[] Specifications { get; set; }

        public class SpecificationSettings
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public MessageRouteConfiguration GetConfiguration()
        {
            var result = new MessageRouteConfiguration(Uri);

            foreach (var specification in Specifications ?? Enumerable.Empty<SpecificationSettings>())
            {
                result.AddSpecification(specification.Name, specification.Value);
            }

            return result;
        }
    }
}