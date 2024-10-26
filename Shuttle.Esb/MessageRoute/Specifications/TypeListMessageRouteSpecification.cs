using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;
using Shuttle.Core.Specification;

namespace Shuttle.Esb;

public class TypeListMessageRouteSpecification : ISpecification<string>
{
    protected readonly List<string> MessageTypes = new();

    public TypeListMessageRouteSpecification(params string[] messageTypes)
        : this((IEnumerable<string>)messageTypes)
    {
    }

    public TypeListMessageRouteSpecification(IEnumerable<string> messageTypes)
    {
        MessageTypes.AddRange(messageTypes);
    }

    public TypeListMessageRouteSpecification(string value)
    {
        var typeNames = Guard.AgainstNullOrEmptyString(value).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var typeName in typeNames)
        {
            var type = Type.GetType(typeName);

            if (type == null)
            {
                throw new MessageRouteSpecificationException(string.Format(Resources.TypeListMessageRouteSpecificationUnknownType, typeName));
            }

            MessageTypes.Add(Guard.AgainstNullOrEmptyString(type.FullName));
        }
    }

    public bool IsSatisfiedBy(string messageType)
    {
        return MessageTypes.Contains(Guard.AgainstNull(messageType));
    }
}