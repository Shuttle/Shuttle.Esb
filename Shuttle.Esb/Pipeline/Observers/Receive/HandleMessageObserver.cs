using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb;

public interface IHandleMessageObserver : IPipelineObserver<OnHandleMessage>
{
    event EventHandler<HandlerExceptionEventArgs>? HandlerException;
    event EventHandler<MessageNotHandledEventArgs>? MessageNotHandled;
}

public class HandleMessageObserver : IHandleMessageObserver
{
    private readonly IMessageHandlerInvoker _messageHandlerInvoker;
    private readonly ISerializer _serializer;
    private readonly ServiceBusOptions _serviceBusOptions;

    public HandleMessageObserver(IOptions<ServiceBusOptions> serviceBusOptions, IMessageHandlerInvoker messageHandlerInvoker, ISerializer serializer)
    {
        _serviceBusOptions = Guard.AgainstNull(Guard.AgainstNull(serviceBusOptions).Value);
        _messageHandlerInvoker = Guard.AgainstNull(messageHandlerInvoker);
        _serializer = Guard.AgainstNull(serializer);
    }

    public event EventHandler<MessageNotHandledEventArgs>? MessageNotHandled;
    public event EventHandler<HandlerExceptionEventArgs>? HandlerException;

    public async Task ExecuteAsync(IPipelineContext<OnHandleMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var processingStatus = state.GetProcessingStatus();

        if (processingStatus == ProcessingStatus.Ignore || processingStatus == ProcessingStatus.MessageHandled)
        {
            return;
        }

        var transportMessage = Guard.AgainstNull(state.GetTransportMessage());

        if (transportMessage.HasExpired())
        {
            return;
        }

        var message = Guard.AgainstNull(state.GetMessage());
        var errorQueue = state.GetErrorQueue();

        try
        {
            var messageHandlerInvokeResult = await _messageHandlerInvoker.InvokeAsync(pipelineContext).ConfigureAwait(false);

            state.SetMessageHandlerInvokeResult(messageHandlerInvokeResult);

            if (messageHandlerInvokeResult.Invoked)
            {
                return;
            }

            MessageNotHandled?.Invoke(this, new(pipelineContext, transportMessage, message));

            if (_serviceBusOptions.RemoveMessagesNotHandled)
            {
                return;
            }

            if (errorQueue == null)
            {
                throw new InvalidOperationException(string.Format(Resources.MessageNotHandledMissingErrorQueueFailure, message.GetType().FullName, transportMessage.MessageId));
            }

            transportMessage.RegisterFailure(string.Format(Resources.MessageNotHandledFailure, message.GetType().FullName, transportMessage.MessageId, errorQueue.Uri));

            await using (var stream = await _serializer.SerializeAsync(transportMessage).ConfigureAwait(false))
            {
                await errorQueue.EnqueueAsync(transportMessage, stream).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            var exception = ex.TrimLeading<TargetInvocationException>();

            HandlerException?.Invoke(this, new(pipelineContext, transportMessage, message, exception));

            throw exception;
        }
    }
}