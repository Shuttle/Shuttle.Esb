using System;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public class DeserializationExceptionEventArgs : PipelineContextEventArgs
{
    public DeserializationExceptionEventArgs(IPipelineContext pipelineContext, IQueue workQueue, IQueue errorQueue, Exception exception)
        : base(pipelineContext)
    {
        WorkQueue = workQueue;
        ErrorQueue = errorQueue;
        Exception = exception;
    }

    public IQueue WorkQueue { get; }
    public IQueue ErrorQueue { get; }
    public Exception Exception { get; }
}