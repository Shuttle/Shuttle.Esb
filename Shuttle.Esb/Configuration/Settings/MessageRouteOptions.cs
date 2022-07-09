using System;
using System.Collections.Generic;
using System.Linq;

namespace Shuttle.Esb
{
    public class MessageRouteOptions
    {
        public string Uri { get; set; }

        public List<SpecificationSettings> Specifications { get; set; } = new List<SpecificationSettings>();

        public class SpecificationSettings
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}