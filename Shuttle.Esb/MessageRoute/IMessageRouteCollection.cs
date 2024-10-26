using System;
using System.Collections.Generic;

namespace Shuttle.Esb;

public interface IMessageRouteCollection : IEnumerable<IMessageRoute>
{
    IMessageRouteCollection Add(IMessageRoute messageRoute);
    IMessageRoute? Find(Uri uri);
    IMessageRoute? Find(string uri);
    IMessageRoute? Find(IQueue queue);

    List<IMessageRoute> FindAll(string messageType);
}