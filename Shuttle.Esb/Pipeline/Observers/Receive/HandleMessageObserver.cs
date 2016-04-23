using System;
using System.Reflection;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class HandleMessageObserver :
		IPipelineObserver<OnHandleMessage>
	{
		private DateTime _expiryDateImplementation = new DateTime(2016, 04, 23);
		private readonly ILog _log;

		public HandleMessageObserver()
		{
			_log = Log.For(this);
		}

		public void Execute(OnHandleMessage pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var processingStatus = state.GetProcessingStatus();

			if (processingStatus == ProcessingStatus.Ignore || processingStatus == ProcessingStatus.MessageHandled)
			{
				return;
			}

			var bus = state.GetServiceBus();
			var transportMessage = state.GetTransportMessage();
			var message = state.GetMessage();

			if (transportMessage.ExpiryDate > _expiryDateImplementation && transportMessage.ExpiryDate < DateTime.Now)
			{
				bus.Events.OnMessageExpired(this,
					new MessageNotHandledEventArgs(
						pipelineEvent,
						state.GetWorkQueue(),
						state.GetErrorQueue(),
						transportMessage,
						message));
				return;
			}

			try
			{
				var messageHandlerInvokeResult = bus.Configuration.MessageHandlerInvoker.Invoke(pipelineEvent);

				if (messageHandlerInvokeResult.Invoked)
				{
					state.SetMessageHandler(messageHandlerInvokeResult.MessageHandler);
				}
				else
				{
					bus.Events.OnMessageNotHandled(this,
						new MessageNotHandledEventArgs(
							pipelineEvent,
							state.GetWorkQueue(),
							state.GetErrorQueue(),
							transportMessage,
							message));

					if (!bus.Configuration.RemoveMessagesNotHandled)
					{
						var error = string.Format(EsbResources.MessageNotHandledFailure, message.GetType().FullName,
							transportMessage.MessageId, state.GetErrorQueue().Uri.Secured());

						_log.Error(error);

						transportMessage.RegisterFailure(error);

						using (var stream = bus.Configuration.Serializer.Serialize(transportMessage))
						{
							state.GetErrorQueue().Enqueue(transportMessage, stream);
						}
					}
					else
					{
						_log.Warning(string.Format(EsbResources.MessageNotHandledIgnored,
							message.GetType().FullName,
							transportMessage.MessageId));
					}
				}
			}
			catch (Exception ex)
			{
				var exception = ex.TrimLeading<TargetInvocationException>();

				bus.Events.OnHandlerException(
					this,
					new HandlerExceptionEventArgs(
						pipelineEvent,
						transportMessage,
						message,
						state.GetWorkQueue(),
						state.GetErrorQueue(),
						exception));

				throw exception;
			}
		}
	}
}