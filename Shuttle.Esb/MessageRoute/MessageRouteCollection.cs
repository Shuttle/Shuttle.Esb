using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class MessageRouteCollection : IMessageRouteCollection
    {
        private readonly List<IMessageRoute> _messageRoutes = new List<IMessageRoute>();

        public IMessageRouteCollection Add(IMessageRoute messageRoute)
        {
            Guard.AgainstNull(messageRoute, nameof(messageRoute));

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
            Guard.AgainstNull(messageType, nameof(messageType));

            return _messageRoutes.Where(map => map.IsSatisfiedBy(messageType)).ToList();
        }

        public IMessageRoute Find(Uri uri)
        {
            Guard.AgainstNull(uri, nameof(uri));

            return Find(uri.ToString());
        }

        public IMessageRoute Find(string uri)
        {
            return _messageRoutes.Find(map => map.Uri.ToString()
                .Equals(uri, StringComparison.InvariantCultureIgnoreCase));
        }

        public IMessageRoute Find(IQueue queue)
        {
            Guard.AgainstNull(queue, nameof(queue));

            return Find(queue.Uri.ToString());
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
}