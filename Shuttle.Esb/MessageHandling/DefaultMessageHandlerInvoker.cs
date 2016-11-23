using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DefaultMessageHandlerInvoker : IMessageHandlerInvoker
    {
        private readonly IComponentContainer _container;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IServiceBusConfiguration _configuration;
        private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);
        private static readonly object _lock = new object();
        private readonly Dictionary<Type, ContextMethod> _cache = new Dictionary<Type, ContextMethod>();

        public DefaultMessageHandlerInvoker(IComponentContainer container, IServiceBusConfiguration configuration, IPipelineFactory pipelineFactory, ISubscriptionManager subscriptionManager)
        {
            Guard.AgainstNull(container, "container");
            Guard.AgainstNull(configuration, "configuration");
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");
            Guard.AgainstNull(subscriptionManager, "subscriptionManager");

            _container = container;
            _configuration = configuration;
            _pipelineFactory = pipelineFactory;
            _subscriptionManager = subscriptionManager;
        }

        public MessageHandlerInvokeResult Invoke(IPipelineEvent pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, "pipelineEvent");

            var state = pipelineEvent.Pipeline.State;
            var message = state.GetMessage();
            var handler = _container.Resolve(MessageHandlerType.MakeGenericType(message.GetType()));

            if (handler == null)
            {
                return MessageHandlerInvokeResult.InvokeFailure();
            }

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

                var handlerContext = Activator.CreateInstance(contextMethod.ContextType, _configuration, _pipelineFactory, _subscriptionManager, transportMessage, message,
                    state.GetActiveState());

                contextMethod.Method.Invoke(handler, new[] { handlerContext });
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