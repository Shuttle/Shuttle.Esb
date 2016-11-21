using System;
using System.IO;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public static class PipelineStateExtensions
	{
	    public static IThreadState GetActiveState(this IState state)
		{
			return state.Get<IThreadState>(StateKeys.ActiveState);
		}

		public static IQueue GetWorkQueue(this IState state)
		{
			return state.Get<IQueue>(StateKeys.WorkQueue);
		}

		public static IQueue GetDeferredQueue(this IState state)
		{
			return state.Get<IQueue>(StateKeys.DeferredQueue);
		}

		public static IQueue GetErrorQueue(this IState state)
		{
			return state.Get<IQueue>(StateKeys.ErrorQueue);
		}

		public static int GetMaximumFailureCount(this IState state)
		{
			return state.Get<int>(StateKeys.MaximumFailureCount);
		}

		public static TimeSpan[] GetDurationToIgnoreOnFailure(this IState state)
		{
			return state.Get<TimeSpan[]>(StateKeys.DurationToIgnoreOnFailure);
		}

		public static void SetTransportMessage(this IState state, TransportMessage value)
		{
			state.Replace(StateKeys.TransportMessage, value);
		}

		public static TransportMessage GetTransportMessage(this IState state)
		{
			return state.Get<TransportMessage>(StateKeys.TransportMessage);
		}

		public static void SetTransportMessageReceived(this IState state, TransportMessage value)
		{
			state.Replace(StateKeys.TransportMessageReceived, value);
		}

		public static TransportMessage GetTransportMessageReceived(this IState state)
		{
			return state.Get<TransportMessage>(StateKeys.TransportMessageReceived);
		}

		public static void SetProcessingStatus(this IState state, ProcessingStatus processingStatus)
		{
			state.Replace(StateKeys.ProcessingStatus, processingStatus);
		}

		public static ProcessingStatus GetProcessingStatus(this IState state)
		{
			return state.Get<ProcessingStatus>(StateKeys.ProcessingStatus);
		}

		public static void SetReceivedMessage(this IState state, ReceivedMessage receivedMessage)
		{
			state.Replace(StateKeys.ReceivedMessage, receivedMessage);
		}

		public static ReceivedMessage GetReceivedMessage(this IState state)
		{
			return state.Get<ReceivedMessage>(StateKeys.ReceivedMessage);
		}

		public static void SetTransportMessageStream(this IState state, Stream value)
		{
			state.Replace(StateKeys.TransportMessageStream, value);
		}

		public static Stream GetTransportMessageStream(this IState state)
		{
			return state.Get<Stream>(StateKeys.TransportMessageStream);
		}

		public static byte[] GetMessageBytes(this IState state)
		{
			return state.Get<byte[]>(StateKeys.MessageBytes);
		}

		public static void SetMessageBytes(this IState state, byte[] bytes)
		{
			state.Replace(StateKeys.MessageBytes, bytes);
		}

		public static object GetMessage(this IState state)
		{
			return state.Get<object>(StateKeys.Message);
		}

		public static void SetMessage(this IState state, object message)
		{
			state.Replace(StateKeys.Message, message);
		}

		public static void SetTransportMessageContext(this IState state, TransportMessageConfigurator configurator)
		{
			state.Replace(StateKeys.TransportMessageConfigurator, configurator);
		}

		public static void SetAvailableWorker(this IState state, AvailableWorker value)
		{
			state.Replace(StateKeys.AvailableWorker, value);
		}

		public static AvailableWorker GetAvailableWorker(this IState state)
		{
			return state.Get<AvailableWorker>(StateKeys.AvailableWorker);
		}

		public static bool GetTransactionComplete(this IState state)
		{
			return state.Get<bool>(StateKeys.TransactionComplete);
		}

		public static void SetTransactionComplete(this IState state)
		{
			state.Replace(StateKeys.TransactionComplete, true);
		}

		public static void SetWorking(this IState state)
		{
			state.Replace(StateKeys.Working, true);
		}

		public static bool GetWorking(this IState state)
		{
			return state.Get<bool>(StateKeys.Working);
		}

		public static void ResetWorking(this IState state)
		{
			state.Replace(StateKeys.Working, false);
		}

		public static ITransactionScope GetTransactionScope(this IState state)
		{
			return state.Get<ITransactionScope>(StateKeys.TransactionScope);
		}

		public static void SetTransactionScope(this IState state, ITransactionScope scope)
		{
			state.Replace(StateKeys.TransactionScope, scope);
		}

		public static void SetMessageHandler(this IState state, object handler)
		{
			state.Replace(StateKeys.MessageHandler, handler);
		}

		public static object GetMessageHandler(this IState state)
		{
			return state.Get<object>(StateKeys.MessageHandler);
		}

		public static void SetWorkQueue(this IState state, IQueue queue)
		{
			state.Add(StateKeys.WorkQueue, queue);
		}

		public static void SetDeferredQueue(this IState state, IQueue queue)
		{
			state.Add(StateKeys.DeferredQueue, queue);
		}

		public static void SetErrorQueue(this IState state, IQueue queue)
		{
			state.Add(StateKeys.ErrorQueue, queue);
		}

		public static void SetMaximumFailureCount(this IState state, int count)
		{
			state.Add(StateKeys.MaximumFailureCount, count);
		}

		public static void SetDurationToIgnoreOnFailure(this IState state, TimeSpan[] timeSpans)
		{
			state.Add(StateKeys.DurationToIgnoreOnFailure, timeSpans);
		}

		public static void SetActiveState(this IState state, IThreadState activeState)
		{
			state.Replace(StateKeys.ActiveState, activeState);
		}

		public static IMessageSender GetHandlerContext(this IState state)
		{
			return state.Get<IMessageSender>(StateKeys.HandlerContext);
		}

		public static void SetHandlerContext(this IState state, IMessageSender handlerContext)
		{
			state.Replace(StateKeys.HandlerContext, handlerContext);
		}

		public static bool GetDeferredMessageReturned(this IState state)
		{
			return state.Get<bool>(StateKeys.DeferredMessageReturned);
		}

		public static void SetDeferredMessageReturned(this IState state, bool deferredMessageReturned)
		{
			state.Replace(StateKeys.DeferredMessageReturned, deferredMessageReturned);
		}
	}
}