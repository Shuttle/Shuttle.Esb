﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class MessageHandlerInvoker : IMessageHandlerInvoker
    {
        private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);
        private static readonly object LockGetHandler = new object();
        private static readonly object LockInvoke = new object();
        private readonly Dictionary<Type, ContextMethodInvoker> _cache = new Dictionary<Type, ContextMethodInvoker>();
        private readonly IServiceProvider _provider;
        private readonly IMessageSender _messageSender;

        private readonly Dictionary<Type, Dictionary<int, object>> _threadHandlers =
            new Dictionary<Type, Dictionary<int, object>>();

        public MessageHandlerInvoker(IServiceProvider provider, IMessageSender messageSender)
        {
            Guard.AgainstNull(provider, nameof(provider));
            Guard.AgainstNull(messageSender, nameof(messageSender));

            _provider = provider;
            _messageSender = messageSender;
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
                ContextMethodInvoker contextMethod;

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

                        contextMethod = new ContextMethodInvoker(
                            messageType,
                            handler.GetType()
                                .GetInterfaceMap(MessageHandlerType.MakeGenericType(messageType))
                                .TargetMethods.SingleOrDefault()
                        );

                        _cache.Add(messageType, contextMethod);
                    }
                }

                var handlerContext = contextMethod.CreateHandlerContext(_messageSender, transportMessage, message, state.GetCancellationToken());

                state.SetHandlerContext(handlerContext);

                contextMethod.Invoke(handler, handlerContext);
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
                    handler = _provider.GetService(MessageHandlerType.MakeGenericType(messageType));
                    instances.Add(managedThreadId, handler);
                }

                return handler;
            }
        }
    }

    internal class ContextMethodInvoker
    {
        private static readonly Type HandlerContextType = typeof(HandlerContext<>);

        private readonly ConstructorInvokeHandler _constructorInvoker;

        private readonly InvokeHandler _invoker;

        public ContextMethodInvoker(Type messageType, MethodInfo methodInfo)
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

            dynamicMethod = new DynamicMethod(string.Empty,
                typeof(void), new[] { typeof(object), typeof(object) },
                HandlerContextType.Module);

            il = dynamicMethod.GetILGenerator();
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

        public object CreateHandlerContext(IMessageSender messageSender, TransportMessage transportMessage, object message, CancellationToken cancellationToken)
        {
            return _constructorInvoker(messageSender, transportMessage, message, cancellationToken);
        }

        private delegate object ConstructorInvokeHandler(IMessageSender messageSender, TransportMessage transportMessage, object message, CancellationToken cancellationToken);

        private delegate void InvokeHandler(object handler, object handlerContext);
    }
}