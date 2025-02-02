using System.Collections.Generic;

namespace Shuttle.Esb;

public class MessageRouteOptions
{
    public List<SpecificationOptions> Specifications { get; set; } = new();
    public string Uri { get; set; } = default!;

    public class SpecificationOptions
    {
        public string Name { get; set; } = default!;
        public string Value { get; set; } = default!;
    }
}