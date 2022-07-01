using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class BrokerEndpointEmptyEventArgs : PipelineEventEventArgs
    {
        public BrokerEndpointEmptyEventArgs(IPipelineEvent pipelineEvent, IBrokerEndpoint brokerEndpoint)
            : base(pipelineEvent)
        {
            BrokerEndpoint = brokerEndpoint;
        }

        public IBrokerEndpoint BrokerEndpoint { get; }
    }
}