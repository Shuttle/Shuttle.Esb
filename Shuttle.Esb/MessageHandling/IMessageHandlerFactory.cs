using System;
using System.Collections.Generic;
using System.Reflection;

namespace Shuttle.Esb
{
	public interface IMessageHandlerFactory
	{
		IMessageHandler GetHandler(object message);
		void ReleaseHandler(IMessageHandler handler);

		IEnumerable<Type> MessageTypesHandled { get; }
	    IMessageHandlerFactory RegisterHandlers();
	    IMessageHandlerFactory RegisterHandlers(Assembly assembly);
	}
}