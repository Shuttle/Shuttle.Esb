using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class MessageNotHandledEventArgs : PipelineEventEventArgs
    {
        public MessageNotHandledEventArgs(IPipelineEvent pipelineEvent, IBrokerEndpoint workBrokerEndpoint, IBrokerEndpoint errorBrokerEndpoint,
            TransportMessage transportMessage, object message)
            : base(pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));
            Guard.AgainstNull(workBrokerEndpoint, nameof(workBrokerEndpoint));
            Guard.AgainstNull(errorBrokerEndpoint, nameof(errorBrokerEndpoint));
            Guard.AgainstNull(transportMessage, nameof(transportMessage));
            Guard.AgainstNull(message, nameof(message));

            WorkBrokerEndpoint = workBrokerEndpoint;
            ErrorBrokerEndpoint = errorBrokerEndpoint;
            TransportMessage = transportMessage;
            Message = message;
        }

        public IBrokerEndpoint WorkBrokerEndpoint { get; }
        public IBrokerEndpoint ErrorBrokerEndpoint { get; }
        public TransportMessage TransportMessage { get; }
        public object Message { get; }
    }
}