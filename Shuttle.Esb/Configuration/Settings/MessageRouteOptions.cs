using System;
using System.Collections.Generic;
using System.Linq;

namespace Shuttle.Esb
{
    public class MessageRouteOptions
    {
        public string Uri { get; set; }

        public List<SpecificationOptions> Specifications { get; set; } = new List<SpecificationOptions>();

        public class SpecificationOptions
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}