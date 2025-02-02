using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public class MessageHandlerInvoker : IMessageHandlerInvoker
{
    private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);
    private readonly Dictionary<Type, HandlerContextConstructorInvoker> _constructorCache = new();
    private readonly Dictionary<Type, MessageHandlerDelegate> _delegates;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IMessageSender _messageSender;
    private readonly Dictionary<Type, ProcessMessageMethodInvoker> _methodCache = new();
    private readonly IServiceProvider _serviceProvider;

    public MessageHandlerInvoker(IServiceProvider serviceProvider, IMessageSender messageSender, IMessageHandlerDelegateProvider messageHandlerDelegateProvider)
    {
        _serviceProvider = Guard.AgainstNull(serviceProvider);
        _messageSender = Guard.AgainstNull(messageSender);
        _delegates = Guard.AgainstNull(Guard.AgainstNull(messageHandlerDelegateProvider).Delegates).ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public async ValueTask<bool> InvokeAsync(IPipelineContext<OnHandleMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var message = Guard.AgainstNull(state.GetMessage());
        var messageType = message.GetType();
        var transportMessage = Guard.AgainstNull(state.GetTransportMessage());

        HandlerContextConstructorInvoker? handlerContextConstructor;

        await _lock.WaitAsync(pipelineContext.Pipeline.CancellationToken).ConfigureAwait(false);

        try
        {
            if (!_constructorCache.TryGetValue(messageType, out handlerContextConstructor))
            {
                handlerContextConstructor = new(messageType);

                _constructorCache.TryAdd(messageType, handlerContextConstructor);
            }
        }
        finally
        {
            _lock.Release();
        }

        var handlerContext = handlerContextConstructor.CreateHandlerContext(Guard.AgainstNull(_messageSender), Guard.AgainstNull(transportMessage), message, pipelineContext.Pipeline.CancellationToken);

        state.SetHandlerContext(handlerContext);

        if (_delegates.TryGetValue(messageType, out var messageHandlerDelegate))
        {
            if (messageHandlerDelegate.HasParameters)
            {
                await (Task)messageHandlerDelegate.Handler.DynamicInvoke(messageHandlerDelegate.GetParameters(_serviceProvider, handlerContext))!;
            }
            else
            {
                await (Task)messageHandlerDelegate.Handler.DynamicInvoke()!;
            }

            return true;
        }

        var handler = _serviceProvider.GetService(MessageHandlerType.MakeGenericType(messageType));

        if (handler == null)
        {
            return false;
        }

        ProcessMessageMethodInvoker? processMessageMethodInvoker;

        await _lock.WaitAsync(pipelineContext.Pipeline.CancellationToken).ConfigureAwait(false);

        try
        {
            if (!_methodCache.TryGetValue(messageType, out processMessageMethodInvoker))
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

                processMessageMethodInvoker = new(methodInfo);

                _methodCache.Add(messageType, processMessageMethodInvoker);
            }
        }
        finally
        {
            _lock.Release();
        }

        await processMessageMethodInvoker.InvokeAsync(handler, handlerContext).ConfigureAwait(false);

        return true;
    }
}