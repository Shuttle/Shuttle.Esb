using System;
using System.Collections.Generic;
using System.Reflection;

namespace Shuttle.Esb
{
	public interface IMessageHandlerFactory
	{
		object GetHandler(object message);
		void ReleaseHandler(object handler);

		IEnumerable<Type> MessageTypesHandled { get; }
	    IMessageHandlerFactory RegisterHandlers();
	    IMessageHandlerFactory RegisterHandlers(Assembly assembly);
	}
}