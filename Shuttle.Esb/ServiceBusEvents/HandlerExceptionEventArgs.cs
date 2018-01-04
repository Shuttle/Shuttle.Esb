using System;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class HandlerExceptionEventArgs : PipelineEventEventArgs
    {
        public HandlerExceptionEventArgs(IPipelineEvent pipelineEvent,
            TransportMessage transportMessage, object message, IQueue workQueue,
            IQueue errorQueue, Exception exception)
            : base(pipelineEvent)
        {
            TransportMessage = transportMessage;
            Message = message;
            WorkQueue = workQueue;
            ErrorQueue = errorQueue;
            Exception = exception;
        }

        public TransportMessage TransportMessage { get; }
        public object Message { get; }
        public IQueue WorkQueue { get; }
        public IQueue ErrorQueue { get; }
        public Exception Exception { get; }
    }
}