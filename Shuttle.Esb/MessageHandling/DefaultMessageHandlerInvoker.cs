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
        private static readonly Type HandlerContextType = typeof(HandlerContext<>);
        private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);
        private static readonly object LockGetHandler = new object();
        private static readonly object LockInvoke = new object();
        private readonly Dictionary<Type, ContextMethod> _cache = new Dictionary<Type, ContextMethod>();
        private readonly IComponentResolver _resolver;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly ISubscriptionManager _subscriptionManager;

        private readonly Dictionary<Type, Dictionary<int, object>> _threadHandlers =
            new Dictionary<Type, Dictionary<int, object>>();

        private readonly ITransportMessageFactory _transportMessageFactory;

        public DefaultMessageHandlerInvoker(IComponentResolver resolver, IPipelineFactory pipelineFactory, ISubscriptionManager subscriptionManager, ITransportMessageFactory transportMessageFactory)
        {
            Guard.AgainstNull(resolver, nameof(resolver));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(subscriptionManager, nameof(subscriptionManager));
            Guard.AgainstNull(transportMessageFactory, nameof(transportMessageFactory));

            _resolver = resolver;
            _pipelineFactory = pipelineFactory;
            _subscriptionManager = subscriptionManager;
            _transportMessageFactory = transportMessageFactory;
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
                    if (!_cache.TryGetValue(messageType, out contextMethod))
                    {
                        var interfaceType = MessageHandlerType.MakeGenericType(messageType);
                        var method =
                            handler.GetType().GetInterfaceMap(interfaceType).TargetMethods.SingleOrDefault();

                        if (method == null)
                        {
                            throw new HandlerMessageMethodMissingException(string.Format(
                                Resources.HandlerMessageMethodMissingException,
                                handler.GetType().FullName,
                                messageType.FullName));
                        }

                        contextMethod = new ContextMethod
                        {
                            ContextType = HandlerContextType.MakeGenericType(messageType),
                            Method = handler.GetType()
                                .GetInterfaceMap(MessageHandlerType.MakeGenericType(messageType))
                                .TargetMethods.SingleOrDefault()
                        };

                        _cache.Add(messageType, contextMethod);
                    }
                }

                var handlerContext = Activator.CreateInstance(contextMethod.ContextType, _transportMessageFactory, _pipelineFactory, _subscriptionManager, transportMessage, message,
                    state.GetCancellationToken());

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
                if (!_threadHandlers.TryGetValue(messageType, out var instances))
                {
                    return;
                }

                instances.Remove(Thread.CurrentThread.ManagedThreadId);
            }
        }

        private object GetHandler(Type messageType)
        {
            lock (LockGetHandler)
            {
                if (!_threadHandlers.TryGetValue(messageType, out var instances))
                {
                    instances = new Dictionary<int, object>();
                    _threadHandlers.Add(messageType, instances);
                }

                var managedThreadId = Thread.CurrentThread.ManagedThreadId;

                if (!instances.TryGetValue(managedThreadId, out var handler))
                {
                    handler = _resolver.Resolve(MessageHandlerType.MakeGenericType(messageType));
                    instances.Add(managedThreadId, handler);
                }

                return handler;
            }
        }
    }

    internal class ContextMethod
    {
        public Type ContextType { get; set; }
        public MethodInfo Method { get; set; }
    }
}