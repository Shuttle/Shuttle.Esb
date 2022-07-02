using System;
using System.Reflection;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb
{
    public interface IHandleMessageObserver : IPipelineObserver<OnHandleMessage>
    {
    }

    public class HandleMessageObserver : IHandleMessageObserver
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IServiceBusEvents _events;
        private readonly ILog _log;
        private readonly IMessageHandlerInvoker _messageHandlerInvoker;
        private readonly ISerializer _serializer;

        public HandleMessageObserver(IServiceBusConfiguration configuration, IServiceBusEvents events,
            IMessageHandlerInvoker messageHandlerInvoker, ISerializer serializer)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(events, nameof(events));
            Guard.AgainstNull(messageHandlerInvoker, nameof(messageHandlerInvoker));
            Guard.AgainstNull(serializer, nameof(serializer));

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
                            state.GetBrokerEndpoint(),
                            state.GetErrorBrokerEndpoint(),
                            transportMessage,
                            message));

                    if (!_configuration.RemoveMessagesNotHandled)
                    {
                        var error = string.Format(Resources.MessageNotHandledFailure, message.GetType().FullName,
                            transportMessage.MessageId, state.GetErrorBrokerEndpoint().Uri.Secured());

                        _log.Error(error);

                        transportMessage.RegisterFailure(error);

                        using (var stream = _serializer.Serialize(transportMessage))
                        {
                            state.GetErrorBrokerEndpoint().Enqueue(transportMessage, stream);
                        }
                    }
                    else
                    {
                        _log.Warning(string.Format(Resources.MessageNotHandledIgnored,
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
                        state.GetBrokerEndpoint(),
                        state.GetErrorBrokerEndpoint(),
                        exception));

                throw exception;
            }
        }
    }
}