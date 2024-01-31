using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class MessageHandlerInvoker : IMessageHandlerInvoker
    {
        private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);
        private static readonly Type AsyncMessageHandlerType = typeof(IAsyncMessageHandler<>);
        private static readonly object LockGetHandler = new object();
        private static readonly object LockInvoke = new object();
        private readonly Dictionary<Type, ContextConstructorInvoker> _constructorCache = new Dictionary<Type, ContextConstructorInvoker>();
        private readonly IMessageSender _messageSender;
        private readonly Dictionary<Type, ContextMethodInvoker> _methodCache = new Dictionary<Type, ContextMethodInvoker>();
        private readonly Dictionary<Type, AsyncContextMethodInvoker> _methodCacheAsync = new Dictionary<Type, AsyncContextMethodInvoker>();
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Dictionary<int, object>> _threadAsyncHandlers = new Dictionary<Type, Dictionary<int, object>>();

        private readonly Dictionary<Type, Dictionary<int, object>> _threadHandlers = new Dictionary<Type, Dictionary<int, object>>();

        public MessageHandlerInvoker(IServiceProvider serviceProvider, IMessageSender messageSender)
        {
            Guard.AgainstNull(serviceProvider, nameof(serviceProvider));
            Guard.AgainstNull(messageSender, nameof(messageSender));

            _serviceProvider = serviceProvider;
            _messageSender = messageSender;
        }

        public MessageHandlerInvokeResult Invoke(OnHandleMessage pipelineEvent)
        {
            return InvokeAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task<MessageHandlerInvokeResult> InvokeAsync(OnHandleMessage pipelineEvent)
        {
            return await InvokeAsync(pipelineEvent, false);
        }

        private object GetHandler(Type messageType, bool sync)
        {
            lock (LockGetHandler)
            {
                Dictionary<int, object> instances = null;

                switch (sync)
                {
                    case true when !_threadHandlers.TryGetValue(messageType, out instances):
                    {
                        instances = new Dictionary<int, object>();
                        _threadHandlers.Add(messageType, instances);
                        break;
                    }
                    case false when !_threadAsyncHandlers.TryGetValue(messageType, out instances):
                    {
                        instances = new Dictionary<int, object>();
                        _threadAsyncHandlers.Add(messageType, instances);
                        break;
                    }
                }

                var managedThreadId = Thread.CurrentThread.ManagedThreadId;

                object handler = null;

                switch (sync)
                {
                    case true when !instances.TryGetValue(managedThreadId, out handler):
                    {
                        handler = _serviceProvider.GetService(MessageHandlerType.MakeGenericType(messageType));
                        instances.Add(managedThreadId, handler);
                        break;
                    }
                    case false when !instances.TryGetValue(managedThreadId, out handler):
                    {
                        handler = _serviceProvider.GetService(AsyncMessageHandlerType.MakeGenericType(messageType));
                        instances.Add(managedThreadId, handler);
                        break;
                    }
                }

                return handler;
            }
        }

        private async Task<MessageHandlerInvokeResult> InvokeAsync(IPipelineEvent pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var message = Guard.AgainstNull(state.GetMessage(), StateKeys.Message);
            var messageType = message.GetType();
            var handler = GetHandler(messageType, sync);

            if (handler == null)
            {
                return MessageHandlerInvokeResult.MissingHandler();
            }

            var transportMessage = Guard.AgainstNull(state.GetTransportMessage(), StateKeys.TransportMessage);

            try
            {
                ContextMethodInvoker contextMethod = null;
                AsyncContextMethodInvoker asyncContextMethod = null;
                ContextConstructorInvoker contextConstructor;

                lock (LockInvoke)
                {
                    if (!_constructorCache.TryGetValue(messageType, out contextConstructor))
                    {
                        contextConstructor = new ContextConstructorInvoker(messageType);

                        _constructorCache.Add(messageType, contextConstructor);
                    }

                    if (sync && !_methodCache.TryGetValue(messageType, out contextMethod))
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

                        contextMethod = new ContextMethodInvoker(
                            messageType,
                            handler.GetType()
                                .GetInterfaceMap(MessageHandlerType.MakeGenericType(messageType))
                                .TargetMethods.SingleOrDefault()
                        );

                        _methodCache.Add(messageType, contextMethod);
                    }

                    if (!sync && !_methodCacheAsync.TryGetValue(messageType, out asyncContextMethod))
                    {
                        var interfaceType = AsyncMessageHandlerType.MakeGenericType(messageType);
                        var method = handler.GetType().GetInterfaceMap(interfaceType).TargetMethods.SingleOrDefault();

                        if (method == null)
                        {
                            throw new HandlerMessageMethodMissingException(string.Format(
                                Resources.HandlerMessageMethodMissingException,
                                handler.GetType().FullName,
                                messageType.FullName));
                        }

                        asyncContextMethod = new AsyncContextMethodInvoker(
                            messageType,
                            handler.GetType()
                                .GetInterfaceMap(AsyncMessageHandlerType.MakeGenericType(messageType))
                                .TargetMethods.SingleOrDefault()
                        );

                        _methodCacheAsync.Add(messageType, asyncContextMethod);
                    }
                }

                var handlerContext = contextConstructor.CreateHandlerContext(Guard.AgainstNull(_messageSender, nameof(_messageSender)), Guard.AgainstNull(transportMessage, nameof(transportMessage)), message, pipelineEvent.Pipeline.CancellationToken);

                state.SetHandlerContext(handlerContext);

                if (sync)
                {
                    contextMethod.Invoke(handler, handlerContext);
                }
                else
                {
                    await asyncContextMethod.InvokeAsync(handler, handlerContext).ConfigureAwait(false);
                }
            }
            finally
            {
                if (handler is IReusability { IsReusable: false })
                {
                    ReleaseHandler(messageType, sync);
                }
            }

            return MessageHandlerInvokeResult.InvokedHandler(handler.GetType().AssemblyQualifiedName);
        }

        private void ReleaseHandler(Type messageType, bool sync)
        {
            lock (LockGetHandler)
            {
                Dictionary<int, object> instances = null;

                switch (sync)
                {
                    case true when !_threadHandlers.TryGetValue(messageType, out instances):
                    case false when !_threadAsyncHandlers.TryGetValue(messageType, out instances):
                    {
                        return;
                    }
                    default:
                    {
                        instances?.Remove(Thread.CurrentThread.ManagedThreadId);
                        break;
                    }
                }
            }
        }
    }

    internal class ContextConstructorInvoker
    {
        private static readonly Type HandlerContextType = typeof(HandlerContext<>);

        private readonly ConstructorInvokeHandler _constructorInvoker;

        public ContextConstructorInvoker(Type messageType)
        {
            var dynamicMethod = new DynamicMethod(string.Empty, typeof(object),
                new[]
                {
                    typeof(IMessageSender),
                    typeof(TransportMessage),
                    typeof(object),
                    typeof(CancellationToken)
                }, HandlerContextType.Module);

            var il = dynamicMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_3);

            var contextType = HandlerContextType.MakeGenericType(messageType);
            var constructorInfo = contextType.GetConstructor(new[]
            {
                typeof(IMessageSender),
                typeof(TransportMessage),
                messageType,
                typeof(CancellationToken)
            });

            il.Emit(OpCodes.Newobj, constructorInfo);
            il.Emit(OpCodes.Ret);

            _constructorInvoker =
                (ConstructorInvokeHandler)dynamicMethod.CreateDelegate(typeof(ConstructorInvokeHandler));
        }

        public object CreateHandlerContext(IMessageSender messageSender, TransportMessage transportMessage, object message, CancellationToken cancellationToken)
        {
            return _constructorInvoker(messageSender, transportMessage, message, cancellationToken);
        }

        private delegate object ConstructorInvokeHandler(IMessageSender messageSender, TransportMessage transportMessage, object message, CancellationToken cancellationToken);
    }

    internal class ContextMethodInvoker
    {
        private static readonly Type HandlerContextType = typeof(HandlerContext<>);

        private readonly InvokeHandler _invoker;

        public ContextMethodInvoker(Type messageType, MethodInfo methodInfo)
        {
            var dynamicMethod = new DynamicMethod(string.Empty,
                typeof(void), new[] { typeof(object), typeof(object) },
                HandlerContextType.Module);

            var il = dynamicMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);

            il.EmitCall(OpCodes.Callvirt, methodInfo, null);
            il.Emit(OpCodes.Ret);

            _invoker = (InvokeHandler)dynamicMethod.CreateDelegate(typeof(InvokeHandler));
        }

        public void Invoke(object handler, object handlerContext)
        {
            _invoker.Invoke(handler, handlerContext);
        }

        private delegate void InvokeHandler(object handler, object handlerContext);
    }

    internal class AsyncContextMethodInvoker
    {
        private static readonly Type HandlerContextType = typeof(HandlerContext<>);

        private readonly InvokeHandler _invoker;

        public AsyncContextMethodInvoker(Type messageType, MethodInfo methodInfo)
        {
            var dynamicMethod = new DynamicMethod(string.Empty,
                typeof(Task), new[] { typeof(object), typeof(object) },
                HandlerContextType.Module);

            var il = dynamicMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);

            il.EmitCall(OpCodes.Callvirt, methodInfo, null);
            il.Emit(OpCodes.Ret);

            _invoker = (InvokeHandler)dynamicMethod.CreateDelegate(typeof(InvokeHandler));
        }

        public async Task InvokeAsync(object handler, object handlerContext)
        {
            await _invoker.Invoke(handler, handlerContext).ConfigureAwait(false);
        }

        private delegate Task InvokeHandler(object handler, object handlerContext);
    }
}