using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class MessageRouteCollection : IMessageRouteCollection
{
    private readonly List<IMessageRoute> _messageRoutes = [];

    public IMessageRouteCollection Add(IMessageRoute messageRoute)
    {
        Guard.AgainstNull(messageRoute);

        var existing = Find(messageRoute.Uri);

        if (existing == null)
        {
            _messageRoutes.Add(messageRoute);
        }
        else
        {
            foreach (var specification in messageRoute.Specifications)
            {
                existing.AddSpecification(specification);
            }
        }

        return this;
    }

    public List<IMessageRoute> FindAll(string messageType)
    {
        Guard.AgainstNull(messageType);

        return _messageRoutes.Where(map => map.IsSatisfiedBy(messageType)).ToList();
    }

    public IMessageRoute? Find(Uri uri)
    {
        return Find(Guard.AgainstNull(uri).ToString());
    }

    public IMessageRoute? Find(string uri)
    {
        Guard.AgainstNullOrEmptyString(uri);

        return _messageRoutes.Find(map => map.Uri.ToString().Equals(uri, StringComparison.InvariantCultureIgnoreCase));
    }

    public IMessageRoute? Find(IQueue queue)
    {
        return Find(Guard.AgainstNull(queue).Uri.ToString());
    }

    public IEnumerator<IMessageRoute> GetEnumerator()
    {
        return _messageRoutes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}