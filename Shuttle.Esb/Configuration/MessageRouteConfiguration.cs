using System.Collections.Generic;
using System.Collections.ObjectModel;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class MessageRouteConfiguration
{
    private readonly List<MessageRouteSpecificationConfiguration> _specifications = new();

    public MessageRouteConfiguration(string uri)
    {
        Uri = uri;
    }

    public IEnumerable<MessageRouteSpecificationConfiguration> Specifications => new
        ReadOnlyCollection<MessageRouteSpecificationConfiguration>(_specifications);

    public string Uri { get; }

    public void AddSpecification(string name, string value)
    {
        Guard.AgainstNullOrEmptyString(name);
        Guard.AgainstNullOrEmptyString(value);

        _specifications.Add(new(name, value));
    }
}