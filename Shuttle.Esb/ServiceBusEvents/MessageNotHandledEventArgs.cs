using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class MessageNotHandledEventArgs : PipelineEventEventArgs
    {
        public MessageNotHandledEventArgs(IPipelineEvent pipelineEvent, IQueue workQueue, IQueue errorQueue,
            TransportMessage transportMessage, object message)
            : base(pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));
            Guard.AgainstNull(workQueue, nameof(workQueue));
            Guard.AgainstNull(errorQueue, nameof(errorQueue));
            Guard.AgainstNull(transportMessage, nameof(transportMessage));
            Guard.AgainstNull(message, nameof(message));

            WorkQueue = workQueue;
            ErrorQueue = errorQueue;
            TransportMessage = transportMessage;
            Message = message;
        }

        public IQueue WorkQueue { get; }
        public IQueue ErrorQueue { get; }
        public TransportMessage TransportMessage { get; }
        public object Message { get; }
    }
}