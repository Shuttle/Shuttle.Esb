using System;
using System.Reflection;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
    public class HandleMessageObserver :
        IPipelineObserver<OnHandleMessage>
    {
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
                        var error = string.Format(ESBResources.MessageNotHandledFailure, message.GetType().FullName,
                            transportMessage.MessageId, state.GetErrorQueue().Uri.Secured());

                        _log.Error(error);

                        transportMessage.RegisterFailure(error);

                        using (var stream = bus.Configuration.Serializer.Serialize(transportMessage))
                        {
                            state.GetErrorQueue().Enqueue(transportMessage.MessageId, stream);
                        }
                    }
                    else
                    {
                        _log.Warning(string.Format(ESBResources.MessageNotHandledIgnored,
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