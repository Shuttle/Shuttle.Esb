using System.Collections.Generic;

namespace Shuttle.Esb
{
    public interface IMessageRouteProvider
    {
        IEnumerable<IMessageRoute> MessageRoutes { get; }
        IEnumerable<string> GetRouteUris(string messageType);
        void Add(IMessageRoute messageRoute);
    }
}