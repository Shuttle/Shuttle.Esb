using System;
using System.Reflection;
using System.Threading.Tasks;
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

        private async Task ExecuteAsync(OnHandleMessage pipelineEvent, bool sync)
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

            var errorQueue = state.GetErrorQueue();

            try
            {
                var messageHandlerInvokeResult = sync
                ? _messageHandlerInvoker.Invoke(pipelineEvent)
                : await _messageHandlerInvoker.InvokeAsync(pipelineEvent).ConfigureAwait(false);

                if (messageHandlerInvokeResult.Invoked)
                {
                    state.SetMessageHandler(messageHandlerInvokeResult.MessageHandler);
                }
                else
                {
                    MessageNotHandled.Invoke(this,
                        new MessageNotHandledEventArgs(pipelineEvent, state.GetWorkQueue(), errorQueue, transportMessage, message));

                    if (!_serviceBusOptions.RemoveMessagesNotHandled)
                    {
                        var failure = string.Format(Resources.MessageNotHandledFailure,
                            message.GetType().FullName, transportMessage.MessageId, errorQueue == null ? Resources.NoErrorQueue : errorQueue.Uri.ToString());

                        if (errorQueue == null)
                        {
                            throw new InvalidOperationException(failure);
                        }

                        transportMessage.RegisterFailure(failure);

                        if (sync)
                        {
                            using (var stream = _serializer.Serialize(transportMessage))
                            {
                                errorQueue.Enqueue(transportMessage, stream);
                            }
                        }
                        else
                        {
                            await using (var stream = await _serializer.SerializeAsync(transportMessage).ConfigureAwait(false))
                            {
                                await errorQueue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var exception = ex.TrimLeading<TargetInvocationException>();

                HandlerException.Invoke(this,
                    new HandlerExceptionEventArgs(pipelineEvent, transportMessage, message, state.GetWorkQueue(),
                        errorQueue, exception));

                throw exception;
            }
        }

        public event EventHandler<MessageNotHandledEventArgs> MessageNotHandled = delegate
        {
        };

        public event EventHandler<HandlerExceptionEventArgs> HandlerException = delegate
        {
        };

        public void Execute(OnHandleMessage pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnHandleMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }
    }
}