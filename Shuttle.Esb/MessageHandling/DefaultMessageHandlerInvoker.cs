using System;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class DefaultMessageHandlerInvoker : IMessageHandlerInvoker
	{
		public MessageHandlerInvokeResult Invoke(IPipelineEvent pipelineEvent)
		{
			Guard.AgainstNull(pipelineEvent, "pipelineEvent");

			var state = pipelineEvent.Pipeline.State;
			var bus = state.GetServiceBus();
			var message = state.GetMessage();
			var handler = bus.Configuration.MessageHandlerFactory.GetHandler(message);

			if (handler == null)
			{
				return MessageHandlerInvokeResult.InvokeFailure();
			}

			try
			{
				var transportMessage = state.GetTransportMessage();
				var messageType = message.GetType();
				var interfaceType = typeof(IMessageHandler<>).MakeGenericType(messageType);
				var method = handler.GetType().GetInterfaceMap(interfaceType).TargetMethods.SingleOrDefault();

				if (method == null)
				{
					throw new ProcessMessageMethodMissingException(string.Format(
						EsbResources.ProcessMessageMethodMissingException,
						handler.GetType().FullName,
						messageType.FullName));
				}

				var contextType = typeof(HandlerContext<>).MakeGenericType(messageType);
				var handlerContext = Activator.CreateInstance(contextType, bus, transportMessage, message, state.GetActiveState());

				method.Invoke(handler, new[] {handlerContext});
			}
			finally
			{
				bus.Configuration.MessageHandlerFactory.ReleaseHandler(handler);
			}

			return MessageHandlerInvokeResult.InvokedHandler(handler);
		}
	}
}