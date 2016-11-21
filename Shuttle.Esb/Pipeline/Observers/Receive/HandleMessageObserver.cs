using System;
using System.Reflection;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class HandleMessageObserver :
		IPipelineObserver<OnHandleMessage>
	{
	    private readonly IMessageHandlerInvoker _messageHandlerInvoker;
	    private readonly ISerializer _serializer;
	    private readonly ILog _log;
	    private readonly IServiceBusConfiguration _configuration;
	    private readonly IServiceBusEvents _events;

	    public HandleMessageObserver(IServiceBusConfiguration configuration, IServiceBusEvents events, IMessageHandlerInvoker messageHandlerInvoker, ISerializer serializer)
		{
            Guard.AgainstNull(configuration, "configuration");
            Guard.AgainstNull(events, "events");
            Guard.AgainstNull(messageHandlerInvoker, "messageHandlerInvoker");
            Guard.AgainstNull(serializer, "serializer");

	        _configuration = configuration;
	        _events = events;
	        _messageHandlerInvoker = messageHandlerInvoker;
		    _serializer = serializer;

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

			var transportMessage = state.GetTransportMessage();
			var message = state.GetMessage();

			if (transportMessage.HasExpired())
			{
				return;
			}

			try
			{
				var messageHandlerInvokeResult = _messageHandlerInvoker.Invoke(pipelineEvent);

				if (messageHandlerInvokeResult.Invoked)
				{
					state.SetMessageHandler(messageHandlerInvokeResult.MessageHandler);
				}
				else
				{
					_events.OnMessageNotHandled(this,
						new MessageNotHandledEventArgs(
							pipelineEvent,
							state.GetWorkQueue(),
							state.GetErrorQueue(),
							transportMessage,
							message));

					if (!_configuration.RemoveMessagesNotHandled)
					{
						var error = string.Format(EsbResources.MessageNotHandledFailure, message.GetType().FullName,
							transportMessage.MessageId, state.GetErrorQueue().Uri.Secured());

						_log.Error(error);

						transportMessage.RegisterFailure(error);

						using (var stream = _serializer.Serialize(transportMessage))
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

				_events.OnHandlerException(
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