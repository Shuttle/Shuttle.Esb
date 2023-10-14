using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class MessageNotHandledEventArgs : PipelineEventEventArgs
    {
        public MessageNotHandledEventArgs(IPipelineEvent pipelineEvent, TransportMessage transportMessage, object message)
            : base(pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));
            Guard.AgainstNull(transportMessage, nameof(transportMessage));
            Guard.AgainstNull(message, nameof(message));

            TransportMessage = transportMessage;
            Message = message;
        }

        public TransportMessage TransportMessage { get; }
        public object Message { get; }
    }
}