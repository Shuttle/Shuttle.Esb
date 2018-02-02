using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Shuttle.Core.Contract;
using Shuttle.Core.Container;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class DefaultMessageHandlerInvoker : IMessageHandlerInvoker
    {
        private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);
        private static readonly object LockGetHandler = new object();
        private static readonly object LockInvoke = new object();
        private readonly Dictionary<Type, ContextMethod> _cache = new Dictionary<Type, ContextMethod>();
        private readonly IServiceBusConfiguration _configuration;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly ISubscriptionManager _subscriptionManager;

        private readonly Dictionary<Type, Dictionary<int, object>> _threadHandlers =
            new Dictionary<Type, Dictionary<int, object>>();

        private readonly ITransportMessageFactory _transportMessageFactory;

        public DefaultMessageHandlerInvoker(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;

            _pipelineFactory = configuration.Resolver.Resolve<IPipelineFactory>();
            _subscriptionManager = configuration.Resolver.Resolve<ISubscriptionManager>();
            _transportMessageFactory = configuration.Resolver.Resolve<ITransportMessageFactory>();
        }

        public MessageHandlerInvokeResult Invoke(IPipelineEvent pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            var state = pipelineEvent.Pipeline.State;
            var message = state.GetMessage();
            var messageType = message.GetType();
            var handler = GetHandler(messageType);

            if (handler == null)
            {
                return MessageHandlerInvokeResult.InvokeFailure();
            }

            var transportMessage = state.GetTransportMessage();

            try
            {
                ContextMethod contextMethod;

                lock (LockInvoke)
                {
                    if (!_cache.ContainsKey(messageType))
                    {
                        var interfaceType = typeof(IMessageHandler<>).MakeGenericType(messageType);
                        var method =
                            handler.GetType().GetInterfaceMap(interfaceType).TargetMethods.SingleOrDefault();

                        if (method == null)
                        {
                            throw new HandlerMessageMethodMissingException(string.Format(
                                Resources.HandlerMessageMethodMissingException,
                                handler.GetType().FullName,
                                messageType.FullName));
                        }

                        _cache.Add(messageType, new ContextMethod
                        {
                            ContextType = typeof(HandlerContext<>).MakeGenericType(messageType),
                            Method = handler.GetType()
                                .GetInterfaceMap(typeof(IMessageHandler<>).MakeGenericType(messageType))
                                .TargetMethods.SingleOrDefault()
                        });
                    }

                    contextMethod = _cache[messageType];
                }

                var handlerContext = Activator.CreateInstance(contextMethod.ContextType, _configuration,
                    _transportMessageFactory, _pipelineFactory, _subscriptionManager, transportMessage, message,
                    state.GetActiveState());

                state.SetHandlerContext((IMessageSender)handlerContext);

                contextMethod.Method.Invoke(handler, new[] {handlerContext});
            }
            finally
            {
                if (handler is IReusability reusability && !reusability.IsReusable)
                {
                    ReleaseHandler(messageType);
                }
            }

            return MessageHandlerInvokeResult.InvokedHandler(handler);
        }

        private void ReleaseHandler(Type messageType)
        {
            lock (LockGetHandler)
            {
                if (!_threadHandlers.ContainsKey(messageType))
                {
                    return;
                }

                var instances = _threadHandlers[messageType];
                var managedThreadId = Thread.CurrentThread.ManagedThreadId;

                if (instances.ContainsKey(managedThreadId))
                {
                    instances.Remove(managedThreadId);
                }
            }
        }

        private object GetHandler(Type messageType)
        {
            lock (LockGetHandler)
            {
                if (!_threadHandlers.ContainsKey(messageType))
                {
                    _threadHandlers.Add(messageType, new Dictionary<int, object>());
                }

                var instances = _threadHandlers[messageType];
                var managedThreadId = Thread.CurrentThread.ManagedThreadId;

                if (!instances.ContainsKey(managedThreadId))
                {
                    instances.Add(managedThreadId,
                        _configuration.Resolver.Resolve(MessageHandlerType.MakeGenericType(messageType)));
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