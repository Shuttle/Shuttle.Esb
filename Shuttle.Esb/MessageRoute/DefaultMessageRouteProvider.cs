using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public sealed class DefaultMessageRouteProvider : IMessageRouteProvider
    {
        private readonly IEnumerable<string> _empty = new ReadOnlyCollection<string>(new List<string>());
        private readonly IMessageRouteCollection _messageRoutes = new MessageRouteCollection();

        public IEnumerable<string> GetRouteUris(string messageType)
        {
            var uri =
                _messageRoutes.FindAll(messageType).Select(messageRoute => messageRoute.Uri.ToString())
                    .FirstOrDefault();

            return
                string.IsNullOrEmpty(uri)
                    ? _empty
                    : new List<string> {uri};
        }

        public void Add(IMessageRoute messageRoute)
        {
            Guard.AgainstNull(messageRoute, nameof(messageRoute));

            var existing = _messageRoutes.Find(messageRoute.Uri);

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

        public IEnumerable<IMessageRoute> MessageRoutes => new ReadOnlyCollection<IMessageRoute>(
            new List<IMessageRoute>(_messageRoutes));
    }
}