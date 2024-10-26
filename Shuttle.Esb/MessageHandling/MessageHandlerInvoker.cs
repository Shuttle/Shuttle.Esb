using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public class MessageHandlerInvoker : IMessageHandlerInvoker
{
    private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);
    private static readonly object LockGetHandler = new();
    private static readonly object LockInvoke = new();
    private readonly Dictionary<Type, HandlerContextConstructorInvoker> _constructorCache = new();
    private readonly IMessageSender _messageSender;
    private readonly Dictionary<Type, AsyncContextMethodInvoker> _methodCacheAsync = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, Dictionary<int, object>> _threadAsyncHandlers = new();

    public MessageHandlerInvoker(IServiceProvider serviceProvider, IMessageSender messageSender)
    {
        _serviceProvider = Guard.AgainstNull(serviceProvider);
        _messageSender = Guard.AgainstNull(messageSender);
    }

    private object? GetHandler(Type messageType)
    {
        lock (LockGetHandler)
        {
            if (!_threadAsyncHandlers.TryGetValue(messageType, out var instances))
            {
                instances = new();
                _threadAsyncHandlers.Add(messageType, instances);
            }

            var managedThreadId = Environment.CurrentManagedThreadId;

            if (instances.TryGetValue(managedThreadId, out var handler))
            {
                return handler;
            }

            handler = _serviceProvider.GetService(MessageHandlerType.MakeGenericType(messageType));

            if (handler != null)
            {
                instances.Add(managedThreadId, handler);
            }

            return handler;
        }
    }

    public async Task<MessageHandlerInvokeResult> InvokeAsync(IPipelineContext<OnHandleMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var message = Guard.AgainstNull(state.GetMessage());
        var messageType = message.GetType();
        var handler = GetHandler(messageType);

        if (handler == null)
        {
            return MessageHandlerInvokeResult.MissingHandler();
        }

        var transportMessage = Guard.AgainstNull(state.GetTransportMessage());

        try
        {
            AsyncContextMethodInvoker? asyncContextMethod;
            HandlerContextConstructorInvoker? handlerContextConstructor;

            lock (LockInvoke)
            {
                if (!_constructorCache.TryGetValue(messageType, out handlerContextConstructor))
                {
                    handlerContextConstructor = new(messageType);

                    _constructorCache.Add(messageType, handlerContextConstructor);
                }

                if (!_methodCacheAsync.TryGetValue(messageType, out asyncContextMethod))
                {
                    var interfaceType = MessageHandlerType.MakeGenericType(messageType);
                    var method = handler.GetType().GetInterfaceMap(interfaceType).TargetMethods.SingleOrDefault();

                    if (method == null)
                    {
                        throw new MessageHandlerInvokerException(string.Format(Resources.HandlerMessageMethodMissingException, handler.GetType().FullName, messageType.FullName));
                    }

                    var methodInfo = handler.GetType().GetInterfaceMap(MessageHandlerType.MakeGenericType(messageType)).TargetMethods.SingleOrDefault();

                    if (methodInfo == null)
                    {
                        throw new MessageHandlerInvokerException(string.Format(Resources.HandlerMessageMethodMissingException, handler.GetType().FullName, messageType.FullName));
                    }

                    asyncContextMethod = new(methodInfo);

                    _methodCacheAsync.Add(messageType, asyncContextMethod);
                }
            }

            var handlerContext = handlerContextConstructor.CreateHandlerContext(Guard.AgainstNull(_messageSender), Guard.AgainstNull(transportMessage), message, pipelineContext.Pipeline.CancellationToken);

            state.SetHandlerContext(handlerContext);

            await asyncContextMethod.InvokeAsync(handler, handlerContext).ConfigureAwait(false);
        }
        finally
        {
            if (handler is IReusability { IsReusable: false })
            {
                ReleaseHandler(messageType);
            }
        }

        return MessageHandlerInvokeResult.InvokedHandler(handler.GetType().AssemblyQualifiedName ?? throw new MessageHandlerInvokerException(string.Format(Resources.AssemblyQualifiedNameException, handler.GetType().Name)));
    }

    private void ReleaseHandler(Type messageType)
    {
        lock (LockGetHandler)
        {
            if (!_threadAsyncHandlers.TryGetValue(messageType, out Dictionary<int, object>? instances))
            {
                return;
            }

            instances.Remove(Environment.CurrentManagedThreadId);
        }
    }
}

internal class HandlerContextConstructorInvoker
{
    private static readonly Type HandlerContextType = typeof(HandlerContext<>);

    private readonly ConstructorInvokeHandler _constructorInvoker;

    public HandlerContextConstructorInvoker(Type messageType)
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

        if (constructorInfo == null)
        {
            throw new MessageHandlerInvokerException(string.Format(Resources.HandlerContextConstructorMissingException, contextType.FullName));
        }

        il.Emit(OpCodes.Newobj, constructorInfo);
        il.Emit(OpCodes.Ret);

        _constructorInvoker = (ConstructorInvokeHandler)dynamicMethod.CreateDelegate(typeof(ConstructorInvokeHandler));
    }

    public object CreateHandlerContext(IMessageSender messageSender, TransportMessage transportMessage, object message, CancellationToken cancellationToken)
    {
        return _constructorInvoker(messageSender, transportMessage, message, cancellationToken);
    }

    private delegate object ConstructorInvokeHandler(IMessageSender messageSender, TransportMessage transportMessage, object message, CancellationToken cancellationToken);
}

internal class AsyncContextMethodInvoker
{
    private static readonly Type HandlerContextType = typeof(HandlerContext<>);

    private readonly InvokeHandler _invoker;

    public AsyncContextMethodInvoker(MethodInfo methodInfo)
    {
        var dynamicMethod = new DynamicMethod(string.Empty, typeof(Task), new[] { typeof(object), typeof(object) }, HandlerContextType.Module);

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