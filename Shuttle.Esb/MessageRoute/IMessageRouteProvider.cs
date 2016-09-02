using System.Collections.Generic;

namespace Shuttle.Esb
{
	public interface IMessageRouteProvider
	{
		IEnumerable<string> GetRouteUris(string messageType);
		void Add(IMessageRoute messageRoute);
		IMessageRoute Find(string uri);
		bool Any();
        IEnumerable<IMessageRoute> MessageRoutes { get; }
	}
}