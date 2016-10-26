using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DefaultMessageHandlerInvoker : IMessageHandlerInvoker
    {
        private static readonly object _lock = new object();
        private readonly Dictionary<Type, ContextMethod> _cache = new Dictionary<Type, ContextMethod>();

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
                lock (_lock)
                {
                    var transportMessage = state.GetTransportMessage();
                    var messageType = message.GetType();

                    if (!_cache.ContainsKey(messageType))
                    {
                        var interfaceType = typeof(IMessageHandler<>).MakeGenericType(messageType);
                        var method =
                            handler.GetType().GetInterfaceMap(interfaceType).TargetMethods.SingleOrDefault();

                        if (method == null)
                        {
                            throw new ProcessMessageMethodMissingException(string.Format(
                                EsbResources.ProcessMessageMethodMissingException,
                                handler.GetType().FullName,
                                messageType.FullName));
                        }

                        _cache.Add(messageType, new ContextMethod
                        {
                            ContextType = typeof(HandlerContext<>).MakeGenericType(messageType),
                            Method = handler.GetType().GetInterfaceMap(typeof(IMessageHandler<>).MakeGenericType(messageType)).TargetMethods.SingleOrDefault()
                        });
                    }

                    var contextMethod = _cache[messageType];

                    var handlerContext = Activator.CreateInstance(contextMethod.ContextType, bus, transportMessage, message,
                        state.GetActiveState());

                    contextMethod.Method.Invoke(handler, new[] { handlerContext });
                }
            }
            finally
            {
                bus.Configuration.MessageHandlerFactory.ReleaseHandler(handler);
            }

            return MessageHandlerInvokeResult.InvokedHandler(handler);
        }
    }

    internal class ContextMethod
    {
        public Type ContextType { get; set; }
        public MethodInfo Method { get; set; }
    }
}