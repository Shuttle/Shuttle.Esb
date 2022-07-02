using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb
{
    public interface IReceiveExceptionObserver : IPipelineObserver<OnPipelineException>
    {
    }

    public class ReceiveExceptionObserver : IReceiveExceptionObserver
    {
        private readonly IServiceBusEvents _events;
        private readonly ILog _log;

        private readonly IServiceBusPolicy _policy;
        private readonly ISerializer _serializer;

        public ReceiveExceptionObserver(IServiceBusEvents events, IServiceBusPolicy policy, ISerializer serializer)
        {
            Guard.AgainstNull(events, nameof(events));
            Guard.AgainstNull(policy, nameof(policy));
            Guard.AgainstNull(serializer, nameof(serializer));

            _events = events;
            _policy = policy;
            _serializer = serializer;
            _log = Log.For(this);
        }

        public void Execute(OnPipelineException pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            _events.OnBeforePipelineExceptionHandled(this, new PipelineExceptionEventArgs(pipelineEvent.Pipeline));

            try
            {
                if (pipelineEvent.Pipeline.ExceptionHandled)
                {
                    return;
                }

                var brokerEndpoint = state.GetBrokerEndpoint() as IQueue;

                if (brokerEndpoint == null)
                {
                    return;
                }

                try
                {
                    var transportMessage = state.GetTransportMessage();
                    var receivedMessage = state.GetReceivedMessage();

                    if (transportMessage == null)
                    {
                        if (receivedMessage != null)
                        {
                            brokerEndpoint.Release(receivedMessage.AcknowledgementToken);

                            _log.Error(string.Format(Resources.ReceivePipelineExceptionMessageReleased,
                                pipelineEvent.Pipeline.Exception.AllMessages()));
                        }
                        else
                        {
                            _log.Error(string.Format(Resources.ReceivePipelineExceptionMessageNotReceived,
                                pipelineEvent.Pipeline.Exception.AllMessages()));
                        }

                        return;
                    }

                    var action = _policy.EvaluateMessageHandlingFailure(pipelineEvent);

                    transportMessage.RegisterFailure(
                        pipelineEvent.Pipeline.Exception.AllMessages(),
                        action.TimeSpanToIgnoreRetriedMessage);

                    using (var stream = _serializer.Serialize(transportMessage))
                    {
                        var handler = state.GetMessageHandler();
                        var handlerFullTypeName = handler != null ? handler.GetType().FullName : "(handler is null)";
                        var currentRetryCount = transportMessage.FailureMessages.Count;

                        var retry = !pipelineEvent.Pipeline.Exception.Contains<UnrecoverableHandlerException>()
                                    &&
                                    action.Retry;

                        if (retry)
                        {
                            _log.Warning(string.Format(Resources.MessageHandlerExceptionWillRetry,
                                handlerFullTypeName,
                                pipelineEvent.Pipeline.Exception.AllMessages(),
                                transportMessage.MessageType,
                                transportMessage.MessageId,
                                currentRetryCount,
                                state.GetMaximumFailureCount()));

                            brokerEndpoint.Send(transportMessage, stream);
                        }
                        else
                        {
                            _log.Error(string.Format(Resources.MessageHandlerExceptionFailure,
                                handlerFullTypeName,
                                pipelineEvent.Pipeline.Exception.AllMessages(),
                                transportMessage.MessageType,
                                transportMessage.MessageId,
                                state.GetMaximumFailureCount(),
                                state.GetErrorBrokerEndpoint().Uri.Secured()));

                            state.GetErrorBrokerEndpoint().Send(transportMessage, stream);
                        }
                    }

                    brokerEndpoint.Acknowledge(receivedMessage.AcknowledgementToken);
                }
                finally
                {
                    pipelineEvent.Pipeline.MarkExceptionHandled();
                    _events.OnAfterPipelineExceptionHandled(this,
                        new PipelineExceptionEventArgs(pipelineEvent.Pipeline));
                }
            }
            finally
            {
                pipelineEvent.Pipeline.Abort();
            }
        }
    }
}