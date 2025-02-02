using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Esb;

public interface IMessageRouteProvider
{
    IEnumerable<IMessageRoute> MessageRoutes { get; }
    Task AddAsync(IMessageRoute messageRoute);
    Task<IEnumerable<string>> GetRouteUrisAsync(string messageType);
}