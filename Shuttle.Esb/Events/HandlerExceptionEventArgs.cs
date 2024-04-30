using System;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class HandlerExceptionEventArgs : PipelineEventEventArgs
    {
        public HandlerExceptionEventArgs(IPipelineEvent pipelineEvent, TransportMessage transportMessage, object message, Exception exception)
            : base(pipelineEvent)
        {
            TransportMessage = transportMessage;
            Message = message;
            Exception = exception;
        }

        public TransportMessage TransportMessage { get; }
        public object Message { get; }
        public Exception Exception { get; }
    }
}