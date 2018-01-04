using System;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class DeserializationExceptionEventArgs : PipelineEventEventArgs
    {
        public DeserializationExceptionEventArgs(IPipelineEvent pipelineEvent, IQueue workQueue, IQueue errorQueue,
            Exception exception)
            : base(pipelineEvent)
        {
            WorkQueue = workQueue;
            ErrorQueue = errorQueue;
            Exception = exception;
        }

        public IQueue WorkQueue { get; }
        public IQueue ErrorQueue { get; }
        public Exception Exception { get; }
    }
}