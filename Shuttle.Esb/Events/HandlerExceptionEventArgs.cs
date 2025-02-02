using System;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public class HandlerExceptionEventArgs : PipelineContextEventArgs
{
    public HandlerExceptionEventArgs(IPipelineContext pipelineContext, TransportMessage transportMessage, object message, Exception exception)
        : base(pipelineContext)
    {
        TransportMessage = transportMessage;
        Message = message;
        Exception = exception;
    }

    public Exception Exception { get; }
    public object Message { get; }
    public TransportMessage TransportMessage { get; }
}