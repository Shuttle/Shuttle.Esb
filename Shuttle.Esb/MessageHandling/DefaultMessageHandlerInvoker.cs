using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DefaultMessageHandlerInvoker : IMessageHandlerInvoker
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IServiceBusConfiguration _configuration;
        private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);
        private static readonly object _lockHandler = new object();
        private static readonly object _lockInvoke = new object();
        private readonly Dictionary<Type, ContextMethod> _cache = new Dictionary<Type, ContextMethod>();
        private readonly Dictionary<Type, Dictionary<int, object>> _threadHandlers = new Dictionary<Type, Dictionary<int, object>>();

        public DefaultMessageHandlerInvoker(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, "configuration");

            _configuration = configuration;

            _pipelineFactory = configuration.Resolver.Resolve<IPipelineFactory>();
            _subscriptionManager = configuration.Resolver.Resolve<ISubscriptionManager>();
        }

        public MessageHandlerInvokeResult Invoke(IPipelineEvent pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, "pipelineEvent");

            var state = pipelineEvent.Pipeline.State;
            var message = state.GetMessage();
            var handler = GetHandler(message.GetType());

            if (handler == null)
            {
                return MessageHandlerInvokeResult.InvokeFailure();
            }

            var transportMessage = state.GetTransportMessage();
            var messageType = message.GetType();

            ContextMethod contextMethod;

            lock (_lockInvoke)
            {
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

                contextMethod = _cache[messageType];
            }

            var handlerContext = Activator.CreateInstance(contextMethod.ContextType, _configuration, _pipelineFactory, _subscriptionManager, transportMessage, message,
                state.GetActiveState());

            contextMethod.Method.Invoke(handler, new[] { handlerContext });

            return MessageHandlerInvokeResult.InvokedHandler(handler);
        }

        private object GetHandler(Type type)
        {
            lock (_lockHandler)
            {
                if (!_threadHandlers.ContainsKey(type))
                {
                    _threadHandlers.Add(type, new Dictionary<int, object>());
                }

                var instances = _threadHandlers[type];
                var managedThreadId = Thread.CurrentThread.ManagedThreadId;

                if (!instances.ContainsKey(managedThreadId))
                {
                    instances.Add(managedThreadId, _configuration.Resolver.Resolve(MessageHandlerType.MakeGenericType(type)));
                }

                return instances[managedThreadId];
            }
        }
    }

    internal class ContextMethod
    {
        public Type ContextType { get; set; }
        public MethodInfo Method { get; set; }
    }
}