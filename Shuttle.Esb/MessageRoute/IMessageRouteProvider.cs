using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public interface IMessageRouteProvider
    {
        IEnumerable<IMessageRoute> MessageRoutes { get; }
        Task<IEnumerable<string>> GetRouteUrisAsync(string messageType);
        IEnumerable<string> GetRouteUris(string messageType);
        void Add(IMessageRoute messageRoute);
    }
}