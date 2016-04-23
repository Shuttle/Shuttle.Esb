using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class TypeListMessageRouteSpecification : ISpecification<string>
	{
		protected readonly List<string> MessageTypes = new List<string>();

		public TypeListMessageRouteSpecification(params string[] messageTypes)
			: this((IEnumerable<string>) messageTypes)
		{
		}

		public TypeListMessageRouteSpecification(IEnumerable<string> messageTypes)
		{
			Guard.AgainstNull(messageTypes, "messageTypes");

			MessageTypes.AddRange(MessageTypes);
		}

		public TypeListMessageRouteSpecification(string value)
		{
			Guard.AgainstNullOrEmptyString(value, "value");

			var typeNames = value.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);

			foreach (var typeName in typeNames)
			{
				Type type = null;

				try
				{
					type = Type.GetType(typeName);
				}
				catch
				{
				}

				if (type == null)
				{
					throw new MessageRouteSpecificationException(
						string.Format(EsbResources.TypeListMessageRouteSpecificationUnknownType, typeName));
				}

				MessageTypes.Add(type.FullName);
			}
		}

		public bool IsSatisfiedBy(string messageType)
		{
			Guard.AgainstNull(messageType, "message");

			return MessageTypes.Contains(messageType);
		}
	}
}