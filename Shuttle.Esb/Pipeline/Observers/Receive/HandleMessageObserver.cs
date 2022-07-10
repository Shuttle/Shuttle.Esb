using System;
using System.Reflection;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb
{
    public interface IHandleMessageObserver : IPipelineObserver<OnHandleMessage>
    {
        event EventHandler<MessageNotHandledEventArgs> MessageNotHandled;
        event EventHandler<HandlerExceptionEventArgs> HandlerException;
    }

    public class HandleMessageObserver : IHandleMessageObserver
    {
        private readonly IMessageHandlerInvoker _messageHandlerInvoker;
        private readonly ISerializer _serializer;
        private readonly ServiceBusOptions _serviceBusOptions;

        public HandleMessageObserver(IOptions<ServiceBusOptions> serviceBusOptions,
            IMessageHandlerInvoker messageHandlerInvoker, ISerializer serializer)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));
            Guard.AgainstNull(messageHandlerInvoker, nameof(messageHandlerInvoker));
            Guard.AgainstNull(serializer, nameof(serializer));

            _serviceBusOptions = serviceBusOptions.Value;
            _messageHandlerInvoker = messageHandlerInvoker;
            _serializer = serializer;
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
                    MessageNotHandled.Invoke(this,
                        new MessageNotHandledEventArgs(pipelineEvent, state.GetWorkQueue(), state.GetErrorQueue(),
                            transportMessage, message));

                    if (!_serviceBusOptions.RemoveMessagesNotHandled)
                    {
                        transportMessage.RegisterFailure(string.Format(Resources.MessageNotHandledFailure,
                            message.GetType().FullName,
                            transportMessage.MessageId, state.GetErrorQueue().Uri.Secured()));

                        using (var stream = _serializer.Serialize(transportMessage))
                        {
                            state.GetErrorQueue().Enqueue(transportMessage, stream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var exception = ex.TrimLeading<TargetInvocationException>();

                HandlerException.Invoke(this,
                    new HandlerExceptionEventArgs(pipelineEvent, transportMessage, message, state.GetWorkQueue(),
                        state.GetErrorQueue(), exception));

                throw exception;
            }
        }

        public event EventHandler<MessageNotHandledEventArgs> MessageNotHandled = delegate
        {
        };

        public event EventHandler<HandlerExceptionEventArgs> HandlerException = delegate
        {
        };
    }
}