using System;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class HandlerExceptionEventArgs : PipelineEventEventArgs
    {
        public HandlerExceptionEventArgs(IPipelineEvent pipelineEvent,
            TransportMessage transportMessage, object message, IBrokerEndpoint workBrokerEndpoint,
            IBrokerEndpoint errorBrokerEndpoint, Exception exception)
            : base(pipelineEvent)
        {
            TransportMessage = transportMessage;
            Message = message;
            WorkBrokerEndpoint = workBrokerEndpoint;
            ErrorBrokerEndpoint = errorBrokerEndpoint;
            Exception = exception;
        }

        public TransportMessage TransportMessage { get; }
        public object Message { get; }
        public IBrokerEndpoint WorkBrokerEndpoint { get; }
        public IBrokerEndpoint ErrorBrokerEndpoint { get; }
        public Exception Exception { get; }
    }
}