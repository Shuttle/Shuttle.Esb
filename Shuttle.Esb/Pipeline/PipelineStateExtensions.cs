using System;
using System.Collections.Generic;
using System.IO;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public static class PipelineStateExtensions
{
    public static bool GetDeferredMessageReturned(this IState state)
    {
        return state.Get<bool>(StateKeys.DeferredMessageReturned);
    }

    public static IQueue? GetDeferredQueue(this IState state)
    {
        return state.Get<IQueue>(StateKeys.DeferredQueue);
    }

    public static IEnumerable<TimeSpan>? GetDurationToIgnoreOnFailure(this IState state)
    {
        return state.Get<IEnumerable<TimeSpan>>(StateKeys.DurationToIgnoreOnFailure);
    }

    public static IQueue? GetErrorQueue(this IState state)
    {
        return state.Get<IQueue>(StateKeys.ErrorQueue);
    }

    public static object? GetHandlerContext(this IState state)
    {
        return state.Get<object>(StateKeys.HandlerContext);
    }

    public static int? GetMaximumFailureCount(this IState state)
    {
        return state.Get<int>(StateKeys.MaximumFailureCount);
    }

    public static object? GetMessage(this IState state)
    {
        return state.Get<object>(StateKeys.Message);
    }

    public static byte[]? GetMessageBytes(this IState state)
    {
        return state.Get<byte[]>(StateKeys.MessageBytes);
    }

    public static bool GetMessageHandlerInvoked(this IState state)
    {
        return state.Get<bool>(StateKeys.MessageHandlerInvokeResult);
    }

    public static ReceivedMessage? GetReceivedMessage(this IState state)
    {
        return state.Get<ReceivedMessage>(StateKeys.ReceivedMessage);
    }

    public static TransportMessage? GetTransportMessage(this IState state)
    {
        return state.Get<TransportMessage>(StateKeys.TransportMessage);
    }

    public static Action<TransportMessageBuilder>? GetTransportMessageBuilder(this IState state)
    {
        return state.Get<Action<TransportMessageBuilder>>(StateKeys.TransportMessageBuilder);
    }

    public static TransportMessage? GetTransportMessageReceived(this IState state)
    {
        return state.Get<TransportMessage>(StateKeys.TransportMessageReceived);
    }

    public static Stream? GetTransportMessageStream(this IState state)
    {
        return state.Get<Stream>(StateKeys.TransportMessageStream);
    }

    public static bool GetWorking(this IState state)
    {
        return state.Contains(StateKeys.Working) && state.Get<bool>(StateKeys.Working);
    }

    public static IQueue? GetWorkQueue(this IState state)
    {
        return state.Get<IQueue>(StateKeys.WorkQueue);
    }

    public static void ResetWorking(this IState state)
    {
        state.Replace(StateKeys.Working, false);
    }

    public static void SetDeferredMessageReturned(this IState state, bool deferredMessageReturned)
    {
        state.Replace(StateKeys.DeferredMessageReturned, deferredMessageReturned);
    }

    public static void SetDeferredQueue(this IState state, IQueue? queue)
    {
        state.Add(StateKeys.DeferredQueue, queue);
    }

    public static void SetDurationToIgnoreOnFailure(this IState state, IEnumerable<TimeSpan> timeSpans)
    {
        state.Add(StateKeys.DurationToIgnoreOnFailure, timeSpans);
    }

    public static void SetErrorQueue(this IState state, IQueue? queue)
    {
        state.Add(StateKeys.ErrorQueue, queue);
    }

    public static void SetHandlerContext(this IState state, object handlerContext)
    {
        state.Replace(StateKeys.HandlerContext, handlerContext);
    }

    public static void SetMaximumFailureCount(this IState state, int count)
    {
        state.Add(StateKeys.MaximumFailureCount, count);
    }

    public static void SetMessage(this IState state, object message)
    {
        state.Replace(StateKeys.Message, message);
    }

    public static void SetMessageBytes(this IState state, byte[] bytes)
    {
        state.Replace(StateKeys.MessageBytes, bytes);
    }

    public static void SetMessageHandlerInvoked(this IState state, bool value)
    {
        state.Replace(StateKeys.MessageHandlerInvokeResult, value);
    }

    public static void SetReceivedMessage(this IState state, ReceivedMessage? receivedMessage)
    {
        state.Replace(StateKeys.ReceivedMessage, receivedMessage);
    }

    public static void SetTransportMessage(this IState state, TransportMessage? value)
    {
        state.Replace(StateKeys.TransportMessage, value);
    }

    public static void SetTransportMessageBuilder(this IState state, Action<TransportMessageBuilder>? builder)
    {
        state.Replace(StateKeys.TransportMessageBuilder, builder);
    }

    public static void SetTransportMessageReceived(this IState state, TransportMessage? value)
    {
        state.Replace(StateKeys.TransportMessageReceived, value);
    }

    public static void SetTransportMessageStream(this IState state, Stream value)
    {
        state.Replace(StateKeys.TransportMessageStream, value);
    }

    public static void SetWorking(this IState state)
    {
        state.Replace(StateKeys.Working, true);
    }

    public static void SetWorkQueue(this IState state, IQueue queue)
    {
        state.Add(StateKeys.WorkQueue, queue);
    }
}