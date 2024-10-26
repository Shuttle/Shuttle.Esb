using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Esb;

public interface IMessageRouteProvider
{
    IEnumerable<IMessageRoute> MessageRoutes { get; }
    void Add(IMessageRoute messageRoute);
    IEnumerable<string> GetRouteUris(string messageType);
    Task<IEnumerable<string>> GetRouteUrisAsync(string messageType);
}