using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public sealed class DefaultMessageRouteProvider : IMessageRouteProvider
	{
		private readonly IMessageRouteCollection _messageRoutes = new MessageRouteCollection();

		public IEnumerable<string> GetRouteUris(string messageType)
		{
			var uri = _messageRoutes.FindAll(messageType).Select(messageRoute => messageRoute.Queue.Uri.ToString()).FirstOrDefault();

			return
				string.IsNullOrEmpty(uri)
					? new List<string>()
					: new List<string> { uri };
		}

		public void Add(IMessageRoute messageRoute)
		{
			Guard.AgainstNull(messageRoute, "messageRoute");

			var existing = _messageRoutes.Find(messageRoute.Queue.Uri);

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
		}

		public IMessageRoute Find(string uri)
		{
			return _messageRoutes.Find(uri);
		}

	    public bool Any()
	    {
	        return _messageRoutes.Any();
	    }
	}
}