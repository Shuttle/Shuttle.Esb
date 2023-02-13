using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;
using Shuttle.Core.Specification;

namespace Shuttle.Esb
{
    public class TypeListMessageRouteSpecification : ISpecification<string>
    {
        protected readonly List<string> MessageTypes = new List<string>();

        public TypeListMessageRouteSpecification(params string[] messageTypes)
            : this((IEnumerable<string>)messageTypes)
        {
        }

        public TypeListMessageRouteSpecification(IEnumerable<string> messageTypes)
        {
            Guard.AgainstNull(messageTypes, nameof(messageTypes));

            MessageTypes.AddRange(MessageTypes);
        }

        public TypeListMessageRouteSpecification(string value)
        {
            Guard.AgainstNullOrEmptyString(value, nameof(value));

            var typeNames = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var reflectionService = new ReflectionService();

            foreach (var typeName in typeNames)
            {
                reflectionService.GetType(typeName).ContinueWith(result =>
                {
                    var type = result.Result;

                    if (type == null)
                    {
                        throw new MessageRouteSpecificationException(
                            string.Format(Resources.TypeListMessageRouteSpecificationUnknownType, typeName));
                    }

                    MessageTypes.Add(type.FullName);
                }).GetAwaiter().GetResult();
            }
        }

        public bool IsSatisfiedBy(string messageType)
        {
            Guard.AgainstNull(messageType, nameof(messageType));

            return MessageTypes.Contains(messageType);
        }
    }
}