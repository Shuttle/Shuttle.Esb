using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public class MessageNotHandledEventArgs : PipelineContextEventArgs
{
    public MessageNotHandledEventArgs(IPipelineContext pipelineContext, TransportMessage transportMessage, object message)
        : base(pipelineContext)
    {
        TransportMessage = Guard.AgainstNull(transportMessage);
        Message = Guard.AgainstNull(message);
    }

    public object Message { get; }

    public TransportMessage TransportMessage { get; }
}