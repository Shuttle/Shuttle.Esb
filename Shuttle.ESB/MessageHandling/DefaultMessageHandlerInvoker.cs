using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DefaultMessageHandlerInvoker : IMessageHandlerInvoker
    {
        public MessageHandlerInvokeResult Invoke(PipelineEvent pipelineEvent)
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
                var contextType = typeof(HandlerContext<>).MakeGenericType(messageType);
                var method = handler.GetType().GetMethod("ProcessMessage", new[] { contextType });

                if (method == null)
                {
                    throw new ProcessMessageMethodMissingException(string.Format(
                        ESBResources.ProcessMessageMethodMissingException,
                        handler.GetType().FullName,
                        messageType.FullName));
                }

                var handlerContext = Activator.CreateInstance(contextType, bus, transportMessage, message, state.GetActiveState());

                method.Invoke(handler, new[] { handlerContext });
            }
            finally
            {
                bus.Configuration.MessageHandlerFactory.ReleaseHandler(handler);
            }

            return MessageHandlerInvokeResult.InvokedHandler(handler);
        }
    }
}